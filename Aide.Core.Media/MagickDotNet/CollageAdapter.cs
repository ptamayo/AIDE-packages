using ImageMagick;
using Aide.Core.Domain;
using Aide.Core.Interfaces;
using Aide.Core.Media.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ImageMagick.Formats;

namespace Aide.Core.Media.MagickDotNet
{
    public interface ICollageAdapter
    {
        CollageAdapter.Media CreateCollage(IEnumerable<CollageAdapter.CollageImage> collageImages, CollageAdapter.CollageSettings collageSettings);
        IEnumerable<HostedFile> ResizeMediaFiles(IEnumerable<HostedFile> mediaFiles, string outputFolder, string baseUrl, int? newImageWidth = null);
        CollageAdapter.Media CreatePdf(List<HostedFile> mediaFiles, CollageAdapter.PdfSettings pdfSettings);
    }

    public class CollageAdapter : ICollageAdapter
    {
        private readonly IFileSystemAdapter _fsa;
        private readonly int _limitMemoryPercentage;
        private readonly int _collageImageWidth;
        private const int MinImageWidth = 600;
        public const string PdfMimeType = "application/pdf";
        private const string PdfExtension = ".pdf";
        private readonly Density _collagePdfDensity;
        private readonly int _collageImageQuality = 75;
        private const double RightRotationDegrees = 90;
        private readonly ICollection<string> ExcludedFileExtensions = new List<string> { ".xml", ".pdf" };

        // This is initialized in the constructor and represents the total memory in the system
        private readonly ulong _systemMemory;

        public ulong SystemMemory
        {
            get { return _systemMemory; }
        }

        public ulong EngineMemory
        {
            get { return ImageMagick.ResourceLimits.Memory; }
        }

        public int LimitMemoryPercentage
        {
            get { return _limitMemoryPercentage; }
        }

        public CollageAdapter(IFileSystemAdapter fsa, CollageAdapterConfig adapterConfig)
        {
            _fsa = fsa ?? throw new ArgumentNullException(nameof(fsa));
            if (adapterConfig == null) throw new ArgumentNullException(nameof(adapterConfig));
            if (adapterConfig.LimitMemoryPercentage < 0) throw new ArgumentOutOfRangeException(nameof(adapterConfig.LimitMemoryPercentage));
            if (adapterConfig.CollageImageWidth < MinImageWidth) throw new ArgumentOutOfRangeException(nameof(adapterConfig.CollageImageWidth), $"The CollageImageWidth is invalid. The minimum value accepted is {MinImageWidth}.");

            // NOTE: There are another interesting properties worth to check in ImageMagick.ResourceLimits
            _systemMemory = ImageMagick.ResourceLimits.Memory;
            _collageImageWidth = adapterConfig.CollageImageWidth;
            _collagePdfDensity = new Density(adapterConfig.CollagePdfDensity);
            _collageImageQuality = adapterConfig.CollageImageQuality;

            // If imageMagickLimitMemoryPercentage is not provided then ImageMagick will take the 50% of the total memory
            if (adapterConfig.LimitMemoryPercentage > 0)
            {
                _limitMemoryPercentage = adapterConfig.LimitMemoryPercentage;
                var ImageMagickMemoryPercentage = new ImageMagick.Percentage(adapterConfig.LimitMemoryPercentage);
                ImageMagick.ResourceLimits.LimitMemory(ImageMagickMemoryPercentage);
            }

            // You need to install Ghostscript if you want to convert EPS/PDF/PS files.
            // For further details: https://github.com/dlemstra/Magick.NET/blob/master/docs/Readme.md
            // Also make sure these files exist in the folder provided: gsdll64.dll and gswin64c.exe
            var gsdll64 = Path.Combine(adapterConfig.GhostscriptDirectory, "gsdll64.dll");
            var gswin64c = Path.Combine(adapterConfig.GhostscriptDirectory, "gswin64c.exe");
            if (!string.IsNullOrWhiteSpace(adapterConfig.GhostscriptDirectory) && _fsa.DirectoryExists(adapterConfig.GhostscriptDirectory)
                && _fsa.FileExists(gsdll64) && _fsa.FileExists(gswin64c))
            {
                MagickNET.SetGhostscriptDirectory(adapterConfig.GhostscriptDirectory);
            }

            // The below line is commented out until I can find a use case that can justify the implementation
            //ImageMagick.MagickNET.SetTempDirectory = ""; // The default value is unknown
        }

        /// <summary>
        /// Resize the image(s) to a new width (or height)
        /// </summary>
        /// <param name="mediaFiles">List of media files</param>
        /// <param name="outputFolder">Output folder</param>
        /// <param name="newImageWidth">New width. If the value is null then it'll use the StandardImageWidth provided in the constructor.</param>
        /// <returns></returns>
        public IEnumerable<HostedFile> ResizeMediaFiles(IEnumerable<HostedFile> mediaFiles, string outputFolder, string baseUrl, int? newImageWidth = null)
        {
            if (mediaFiles == null) throw new ArgumentNullException(nameof(mediaFiles));
            if (mediaFiles.Any(x => string.IsNullOrWhiteSpace(x.Filename))) throw new ArgumentNullException(nameof(mediaFiles));
            if (string.IsNullOrWhiteSpace(outputFolder)) throw new ArgumentNullException(nameof(outputFolder));
            if (!_fsa.DirectoryExists(outputFolder)) throw new ArgumentException($"The output folder does not exist: {outputFolder}", nameof(outputFolder));
            if (newImageWidth.HasValue && newImageWidth.Value < MinImageWidth) throw new ArgumentOutOfRangeException(nameof(newImageWidth), $"The standardImageWidth is invalid. The minimum value accepted is {MinImageWidth}.");

            if (!newImageWidth.HasValue)
            {
                newImageWidth = _collageImageWidth;
            }

            var resizedMediaFiles = new List<HostedFile>();
            if (mediaFiles.Any())
            {
                using (var images = new MagickImageCollection())
                {
                    foreach (var mediaFile in mediaFiles)
                    {
                        var fileInfo = _fsa.GetFileInfo(mediaFile.Filename);
                        if (!ExcludedFileExtensions.Contains(fileInfo.Extension, StringComparer.OrdinalIgnoreCase))
                        {
                            var image = new MagickImage(mediaFile.Filename);

                            // Resize the largest side of the image
                            image = ResizeImageBasedInOrientation(image, newImageWidth.Value);
                            var newFilename = Path.GetFileNameWithoutExtension(fileInfo.Name);
                            newFilename = $"{newFilename}_resized{fileInfo.Extension}";
                            newFilename = _fsa.GenerateUniqueFileName(outputFolder, newFilename);
                            var newFullFilename = Path.Combine(outputFolder, newFilename);
                            image.Write(newFullFilename);
                            mediaFile.Filename = newFullFilename;
                            mediaFile.Url = $"{baseUrl}/{newFilename}";
                        }
                        resizedMediaFiles.Add(mediaFile);
                    }
                }
            }
            return resizedMediaFiles;
        }

        public Media CreateCollage(IEnumerable<CollageImage> collageImages, CollageSettings collageSettings)
        {
            if (collageImages == null) throw new ArgumentNullException(nameof(collageImages));
            if (collageImages.Any(x => string.IsNullOrWhiteSpace(x.Filename))) throw new ArgumentNullException(nameof(collageImages));
            if (collageSettings == null) throw new ArgumentNullException(nameof(collageSettings));

            if (collageImages.Any())
            {
                using (var images = new MagickImageCollection())
                {
                    foreach (var collageImage in collageImages)
                    {
                        var fileInfo = _fsa.GetFileInfo(collageImage.Filename);
                        // IMPORTANT: Only non-excluded file extensions will be processed
                        if (!ExcludedFileExtensions.Contains(fileInfo.Extension, StringComparer.OrdinalIgnoreCase))
                        {
                            var image = new MagickImage(collageImage.Filename);
                            image = FixImageOrientation(image, collageImage.Orientation);
                            image = HomologizeImageWidthBasedInOrientation(image, collageImage.Orientation);
                            images.Add(image);
                        }
                    }

                    HomologizeImageCollectionSize(images);

                    // Geometry
                    var geometry = (MagickGeometry)images[0].BoundingBox;
                    //var geometry = new MagickGeometry();
                    //geometry.Width = images[0].Width;
                    //geometry.Height = images[0].Height;
                    //geometry.Less = true;
                    //geometry.Greater = true;
                    //geometry.FillArea = true;

                    // Montage settings
                    var montageSettings = new MontageSettings
                    {
                        Geometry = geometry,
                        TileGeometry = new MagickGeometry($"{collageSettings.Columns}x"),
                        BackgroundColor = MagickColor.FromRgb(255, 255, 255)
                    };

                    // Create a mosaic from the list of images
                    var media = new Media
                    {
                        MimeType = collageSettings.MimeType,
                        Filename = Path.Combine(collageSettings.OutputFolder, collageSettings.Filename),
                        Url = $"{collageSettings.BaseUrl}/{collageSettings.Filename}",
                        DateCreated = DateTime.UtcNow,
                        DateModified = DateTime.UtcNow
                    };
                    using (var result = images.Montage(montageSettings))
                    {
                        // Resize the collage to a fixed width
                        result.Resize(_collageImageWidth, 0);
                        //result.Minify(); // DO NOT USE MINIFY BECAUSE THE QUALITY GOES DOWN BADLY
                        // Commented out the TRIM because it only removed the outer white space 
                        //result.Trim();
                        // Save the result
                        result.Write(media.Filename);
                        // Save the collage as PDF
                        //result.Write(Path.Combine(collageSettings.OutputFolder, "collage.pdf"));
                        // Save all images in a PDF file (before the collage)
                        //images.Write(Path.Combine(collageSettings.OutputFolder, "images.pdf"));
                        return media;
                    }
                }
            }
            return null;
        }

        public Media CreatePdf(List<HostedFile> mediaFiles, PdfSettings pdfSettings)
        {
            if (mediaFiles == null) throw new ArgumentNullException(nameof(mediaFiles));
            if (mediaFiles.Any(x => string.IsNullOrWhiteSpace(x.Filename))) throw new ArgumentNullException(nameof(mediaFiles));
            if (pdfSettings == null) throw new ArgumentNullException(nameof(pdfSettings));

            if (mediaFiles.Any())
            {
                var newDocumentWidth = pdfSettings.ResizeDocumentWidth.HasValue ? pdfSettings.ResizeDocumentWidth.Value : MinImageWidth;

                using (var images = new MagickImageCollection())
                {
                    foreach (var mediaFile in mediaFiles.Where(x => !x.Filename.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)).OrderBy(o => o.SortPriority))
                    {
                        if (mediaFile.Filename.EndsWith(PdfExtension, StringComparison.OrdinalIgnoreCase))
                        {
                            // TODO: Verify if this works as expected.
                            var pdfInfo = PdfInfo.Create(mediaFile.Filename);

                            // Settings for reading PDF files in the list of media files
                            // This is important because PDF files are loaded diferently of regular image files
                            var readPdfSettings = new MagickReadSettings
                            {
                                Density = _collagePdfDensity, // This is very important to avoid blur on text. Be careful, as higher the number the bigger the size of the file is.
                                //Density = new Density(300, 300), // Settings the density to 300 dpi will create an image with a better quality
                                ColorType = ColorType.Grayscale, // Not sure if this is too userful
                                //Width = newDocumentWidth // TODO: Test this line.
                            };

                            // Convert PDF to multiple images
                            // Reference: https://github.com/dlemstra/Magick.NET/blob/main/docs/ConvertPDF.md#convert-pdf-to-multiple-images
                            MagickImageCollection pages = new MagickImageCollection();
                            pages.Read(mediaFile.Filename, readPdfSettings);
                            foreach (var page in pages)
                            {
                                page.Strip();
                                page.Resize(newDocumentWidth, 0);
                                page.Quality = _collageImageQuality;
                                images.Add(page);
                            }
                        }
                        else
                        {
                            var image = new MagickImage(mediaFile.Filename);
                            image = FixImageOrientation(image, (CollageImageOrientation)mediaFile.DocumentOrientation);
                            image.Resize(newDocumentWidth, 0);
                            image.Quality = _collageImageQuality;
                            images.Add(image);
                        }
                    }

                    // Create a mosaic from the list of images
                    var media = new Media
                    {
                        MimeType = PdfMimeType,
                        Filename = Path.Combine(pdfSettings.OutputFolder, pdfSettings.Filename),
                        Url = $"{pdfSettings.BaseUrl}/{pdfSettings.Filename}",
                        DateCreated = DateTime.UtcNow,
                        DateModified = DateTime.UtcNow
                    };

                    images.Write(media.Filename);
                    return media;
                }
            }
            return null;
        }

        private MagickImage FixImageOrientation(MagickImage image, CollageImageOrientation orientation)
        {
            // If orientation is NA then it won't rotate at all
            if (orientation == CollageImageOrientation.Portrait && image.Width > image.Height)
            {
                image.Rotate(RightRotationDegrees);
            }
            if (orientation == CollageImageOrientation.Landscape && image.Width < image.Height)
            {
                image.Rotate(RightRotationDegrees);
            }
            return image;
        }

        private MagickImage HomologizeImageWidthBasedInOrientation(MagickImage image, CollageImageOrientation orientation)
        {
            if (orientation == CollageImageOrientation.Landscape)
            {
                image.Resize(0, _collageImageWidth);
            }
            else // Here: Portrait and N/A
            {
                image.Resize(_collageImageWidth, 0);
            }
            return image;
        }

        private void HomologizeImageCollectionSize(MagickImageCollection images)
        {
            // Separate landscape and portrait images
            var landscapeImages = (from img in images
                                   where img.Width >= img.Height
                                   select img);

            var portraitImages = (from img in images
                                  where img.Width < img.Height
                                  select img);

            // Determine max width and height of the predominat group
            var maxWidth = 0;
            var maxHeight = 0;
            if (landscapeImages.Count() >= portraitImages.Count())
            {
                maxWidth = landscapeImages.Max(x => x.Width);
                maxHeight = landscapeImages.Max(x => x.Height);
            }
            else
            {
                maxWidth = portraitImages.Max(x => x.Width);
                maxHeight = portraitImages.Max(x => x.Height);
            }

            // Resize smaller images to max (if any)
            foreach (var img in images)
            {
                if (img.Width > img.Height)
                {
                    if (img.Width < maxWidth)
                    {
                        img.Resize(maxWidth, 0);
                        img.Enhance();
                    }
                }
                else if (img.Width <= img.Height && img.Height > maxHeight)
                {
                    img.Resize(0, maxHeight);
                    img.Enhance();
                }
                img.AutoLevel();
                img.AutoGamma();
            }
        }

        private MagickImage ResizeImageBasedInOrientation(MagickImage image, int newImageWidth)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (newImageWidth < MinImageWidth) throw new ArgumentOutOfRangeException(nameof(newImageWidth), $"The standardImageWidth is invalid. The minimum value accepted is {MinImageWidth}.");
            if (image.Width >= image.Height)
            {
                // If the image it's in lanscape then resize the width
                image.Resize(newImageWidth, 0);
            }
            else if (image.Width < image.Height)
            {
                // If the image it's in portrait the resize the height
                image.Resize(0, newImageWidth);
            }
            return image;
        }

        #region Local classes

        public class CollageSettings
        {
            public int Columns { get; set; }
            public string MimeType { get; set; }
            public string Filename { get; set; }
            public string OutputFolder { get; set; }
            public string BaseUrl { get; set; }
        }

        public class PdfSettings
        {
            public string Filename { get; set; }
            public string OutputFolder { get; set; }
            public string BaseUrl { get; set; }
            public int? ResizeDocumentWidth { get; set; }
        }

        public class CollageImage
        {
            public string Filename { get; set; }
            public CollageImageOrientation Orientation { get; set; }
        }

        public class Media
        {
            public int Id { get; set; }
            public string MimeType { get; set; }
            public string Filename { get; set; }
            public string Url { get; set; }
            public string MetadataTitle { get; set; }
            public string MetadataAlt { get; set; }
            public string MetadataCopyright { get; set; }
            public string ChecksumSha1 { get; set; }
            public string ChecksumMd5 { get; set; }
            public DateTime DateCreated { get; set; }
            public DateTime DateModified { get; set; }
        }

        public class CollageAdapterConfig
        {
            public int LimitMemoryPercentage { get; set; }
            public int CollagePdfDensity { get; set; }
            public int CollageImageQuality { get; set; }
            public int CollageImageWidth { get; set; }
            public string GhostscriptDirectory { get; set; }
        }

        #endregion
    }
}
