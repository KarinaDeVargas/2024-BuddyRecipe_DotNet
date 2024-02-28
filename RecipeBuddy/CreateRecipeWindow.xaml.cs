using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace RecipeBuddy
{

    public partial class CreateRecipeWindow : Window
    {
        public List<ingredient> ingredients = new List<ingredient>();
        public List<CombinedIngredientData> combinedIngredients = new List<CombinedIngredientData>();
        
        // Define a property to store the newly created recipe
        public recipe NewRecipe { get; set; }

        // Define a property to store the userId
        private int userId;
        private string fileName;
        private string recipeImageName;

        // Constructor that accepts the userId
        public CreateRecipeWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LvIngredients.ItemsSource = combinedIngredients;
        }

        private void BtnAddImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a dialog for the user to chose a picture
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.jpg; *.png)|*.jpg; *.png|All files (*.*)|*.*";
                openFileDialog.Title = "Select an Image";

                // Show the dialog
                bool? result = openFileDialog.ShowDialog();

                // Process the result
                if (result == true)
                {
                    fileName = openFileDialog.FileName;
                    string selectedFileName = Path.GetFileName(fileName);
                    string imagesFolder = GeneralMethods.GetImagesFolder();

                    // Create the RecipeImages folder if it doesn't exist
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    recipeImageName = $"{userId}_{selectedFileName}";
                    string recipeImagePath = Path.Combine(imagesFolder, recipeImageName);

                    // Check if the file already exists in the RecipeImages folder
                    if (File.Exists(recipeImagePath))
                    {
                        MessageBoxResult overwriteResult = MessageBox.Show("The file already exists. Do you want to overwrite it?", "File Exists", MessageBoxButton.YesNo);

                        if (overwriteResult == MessageBoxResult.No)
                        {
                            return;
                        }
                    }

                    // Copy the selected file to the RecipeImages folder with the new name
                    File.Copy(fileName, recipeImagePath, true);

                    SetRecipeImage(recipeImagePath);
                }
            }
            catch (Exception ex) //TO DO: improve exception handling
            {
                throw new Exception(ex.Message);
            }
        }

        public void SetRecipeImage(string recipeImagePath)
        {
            try
            {
                RecImage.Source = GeneralMethods.GetImage(recipeImagePath);
            }
            catch (ArgumentNullException ex)
            {
                throw new ArgumentNullException(ex.ParamName, ex.InnerException.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddIngredients addIngredient = new AddIngredients();
            addIngredient.ShowDialog();

            if (addIngredient.DialogResult == true)
            {
                ingredients.Add(addIngredient.addedIngredient);

                CombinedIngredientData newCombined = new CombinedIngredientData
                {
                    Ingredient = addIngredient.addedIngredient.ingredientName,
                    Quantity = addIngredient.quantity,
                    Unit = addIngredient.unit,
                };
                combinedIngredients.Add(newCombined);

                LvIngredients.Items.Refresh();

                BtnAdd_Click(sender, e);//auto repeat maybe fix maybe delete?
            }
        }

        private void BtnCreateRecipe_Click(object sender, RoutedEventArgs e)
        {
            // Create a new recipe object and populate its properties from the input fields
            NewRecipe = new recipe
            {
                recipeName = TbxName.Text,
                description = TbxDescription.Text,
                instructions = TbxInstructions.Text,
                recipeimage = recipeImageName,
                userId = this.userId, // Assign the userId
            };

            if (InsertIngredients())
            {
                LblStatus.Text = "Ingredients added";
            } else
            {
                MessageBox.Show("Failed to add ingredients. Something went wrong."); //TO DO: Better error handling.
            }

            if (combinedIngredients.Count() > 0)
            {
                foreach (var ingredient in combinedIngredients)
                {
                    NewRecipe.ingredients = String.Join(",", combinedIngredients.Select(i => i.Ingredient));
                }
            }
            else
            {
                NewRecipe.ingredients = "";
            }

            // Insert the new recipe data into the database
            if (InsertRecipeIntoDatabase(NewRecipe))
            {
                LblStatus.Text = "Recipe created";
                this.DialogResult = true; // Close the window
            }
            else
            {
                MessageBox.Show("Failed to create recipe. Please try again.");
            }
        }

        // Method to insert the new recipe data into the database
        private bool InsertRecipeIntoDatabase(recipe newRecipe)
        {
            try
            {
                // Add the new recipe to the DbContext and save changes
                Globals.dbContext.recipes.Add(newRecipe);
                Globals.dbContext.SaveChanges();

                int repInt = Globals.dbContext.recipes
                    .Where(recipe => recipe.recipeName == newRecipe.recipeName)
                    .Select(recipe => recipe.recipeId)
                    .FirstOrDefault();

                for (int i = 0; i < ingredients.Count; i++)
                {
                    recipeingredient ri = new recipeingredient
                    {
                        recipeId = repInt,
                        ingredientId = ingredients[i].ingredientId,
                        quantity = combinedIngredients[i].Quantity,
                        unit = combinedIngredients[i].Unit,
                        
                    };
                    Globals.dbContext.recipeingredients.Add(ri);
                    Globals.dbContext.SaveChanges();
                }
                return true;
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        Console.WriteLine("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                        MessageBox.Show("Error: " + validationError.ErrorMessage);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                return false;
            }
        }

        private bool InsertIngredients()
        {
            try
            {
                foreach (ingredient i in ingredients)
                {
                    if (Globals.dbContext.ingredients.FirstOrDefault(ingredient => ingredient.ingredientName == i.ingredientName) == null)
                    {
                        Globals.dbContext.ingredients.Add(i);
                        Globals.dbContext.SaveChanges();
                    }
                }
                return true;
            }
            catch (Exception ex) //TO DO: change to system exception only?
            {
                MessageBox.Show("Error: " + ex.Message);
                return false;
            }
        }


        //TO DO: Delete these?
        private void TbxDescription_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void LvIngredients_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }

    //join class of ingredient Name from ingredient, quantity and unit from recipeIngredient
    public class CombinedIngredientData
    {
        public string Ingredient { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
    }
}