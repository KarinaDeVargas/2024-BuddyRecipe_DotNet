using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace RecipeBuddy
{
    // good practice: created for 'code reuse', we can use in different parts of the code
    public static class GeneralMethods //it is static, so we do not need to use "new"
    {
        private const string RECIPE_IMAGES_FOLDER = "RecipeImages"; //

        public static string GetImagesFolder(string recipeImagesFolder = RECIPE_IMAGES_FOLDER) 
        {
            try
            {
                if(recipeImagesFolder == null) throw new ArgumentNullException(nameof(recipeImagesFolder));

                string imagesFolder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\"));
                return Path.Combine(imagesFolder, recipeImagesFolder.Trim());
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }            
        }

        public static BitmapImage GetImage(string recipeImagePath)
        {
            try
            {
                if (recipeImagePath == null) throw new ArgumentNullException(nameof(recipeImagePath));

                if (!recipeImagePath.Trim().Contains(RECIPE_IMAGES_FOLDER) || 
                    !File.Exists(recipeImagePath) ||
                    string.IsNullOrWhiteSpace(recipeImagePath)) return null;
                
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(recipeImagePath.Trim(), UriKind.RelativeOrAbsolute);
                bitmap.EndInit();

                return bitmap;
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //check for special character because we do not want special charac in file names
        public static bool HasSpecialCharacters(string input)
        {
            foreach (char c in input)
            {
                if (!Char.IsLetterOrDigit(c))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
