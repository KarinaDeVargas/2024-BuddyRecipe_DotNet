using Microsoft.VisualStudio.TestTools.UnitTesting;
using RecipeBuddy;
using System;
using System.IO;

namespace RecipeBuddyUnitTests
{
    [TestClass]
    public class GeneralMethodsTest
    {
        private const string recipeImageTest = "\\ImageTest.jpg";

        public string GetRecipeImagesPath()
        {
            return Path.Combine(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\")), "RecipeImages"); //.Replace("UnitTests","")
        }

        [TestMethod]
        public void GetImagesFolder_ReturnsCorrectPath_WhenNoExceptions()
        {
            string expected = GetRecipeImagesPath();
            string result = GeneralMethods.GetImagesFolder();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetImagesFolder_ThrowsArgumentNullException_WhenPassingNull()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GeneralMethods.GetImagesFolder(null));
        }

        [TestMethod]
        public void GetImage_ReturnsValidBitmapImage_WhenValidPath()
        {
            var expected = GetRecipeImagesPath() + recipeImageTest;
            var result = GeneralMethods.GetImage(expected);

            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result.UriSource.OriginalString);
        }

        [TestMethod]
        public void GetImage_TrimsPathCorrectly_BeforeCreatingBitmapImage()
        {
            string imagePathWithSpaces = "     " + GetRecipeImagesPath() + recipeImageTest + "           ";
            string expectedImagePath = GetRecipeImagesPath() + recipeImageTest;

            var result = GeneralMethods.GetImage(imagePathWithSpaces);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedImagePath, result.UriSource.OriginalString.Trim());
        }

        [TestMethod]
        public void GetImage_ReturnsNull_WhenInvalidPath()
        {
            var invalidImagePath = "InvalidPathToImage";

            Assert.IsNull(GeneralMethods.GetImage(invalidImagePath));
        }

        [TestMethod]
        public void GetImage_ReturnsNull_WhenPathIsEmpty()
        {
            string emptyImagePath = string.Empty;

            Assert.IsNull(GeneralMethods.GetImage(emptyImagePath));
        }

        [TestMethod]
        public void GetImage_ReturnsNull_WhenFileDoesNotExist()
        {
            string nonExistentImagePath = "NonExistentPathToImage";

            Assert.IsNull(GeneralMethods.GetImage(nonExistentImagePath));
        }

        [TestMethod]
        public void GetImage_ThrowsArgumentNullException_WhenPathIsNull()
        {
            string nullImagePath = null;
            Assert.ThrowsException<ArgumentNullException>(() => GeneralMethods.GetImage(nullImagePath));
        }

        [TestMethod]
        public void HasSpecialCharacters_ReturnsTrue_WhenStringHasSpecialCharacters()
        {
            string stringWithSpecialChars = "Tes$#%t";

            Assert.IsTrue(GeneralMethods.HasSpecialCharacters(stringWithSpecialChars));
        }

        [TestMethod]
        public void HasSpecialCharacters_ReturnsFalse_WhenStringDoesNotHaveSpecialCharacters()
        {
            string stringWithoutSpecialChars = "Test";

            Assert.IsFalse(GeneralMethods.HasSpecialCharacters(stringWithoutSpecialChars));
        }
    }
}
