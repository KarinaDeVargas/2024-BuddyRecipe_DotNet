using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RecipeBuddy
{
    /// <summary> comment
    /// Interaction logic for ViewRecipe.xaml
    /// </summary>            
    public partial class ViewRecipe : Window
    {
        List<ingredient> ingredients = new List<ingredient>();
        public string statusUpdate { get; set; }
        recipe selRecipe { get; set; }
        public List<CombinedIngredientData> combinedIngredients = new List<CombinedIngredientData>();
        public bool update = false;

        public ViewRecipe(recipe recipe)
        {
            InitializeComponent();
            Globals.dbContext = new RecipeDBEntities();
            dgRecipeView.ItemsSource = combinedIngredients;

            update = false;

            if (recipe != null)
            {
                selRecipe = recipe;
                txtbRecipeName.Text = selRecipe.recipeName;
                txtbRecipeDescription.Text = selRecipe.description;
                txtbRecipeInstructions.Text = selRecipe.instructions;

                if (!string.IsNullOrWhiteSpace(selRecipe.recipeimage))
                {
                    SetRecipeImage(selRecipe.recipeimage);
                }

                List<int> ingredIds = new List<int>();
                int recipeid = selRecipe.recipeId;
                ingredIds = Globals.dbContext.recipeingredients
                    .Where(recipeingredient => recipeingredient.recipeId == selRecipe.recipeId)
                    .Select(recipeingredient => recipeingredient.ingredientId)
                    .ToList();
                foreach (var i in ingredIds)
                {
                    CombinedIngredientData newIngred = new CombinedIngredientData
                    {
                        Ingredient = Globals.dbContext.ingredients
                            .Where(ingredient => ingredient.ingredientId == i)
                            .Select(ingredient => ingredient.ingredientName)
                            .FirstOrDefault(),
                        Quantity = Globals.dbContext.recipeingredients
                            .Where(recipeingredient => recipeingredient.ingredientId == i && recipeingredient.recipeId == recipeid)
                            .Select(recipeingredient => recipeingredient.quantity)
                            .FirstOrDefault(),
                        Unit = Globals.dbContext.recipeingredients
                            .Where(recipeingredient => recipeingredient.ingredientId == i && recipeingredient.recipeId == recipeid)
                            .Select(recipeingredient => recipeingredient.unit)
                            .FirstOrDefault(),
                    };
                    ingredients = Globals.dbContext.ingredients.Where(ingredients => ingredients.ingredientId == i).ToList();
                    combinedIngredients.Add(newIngred);
                    dgRecipeView.Items.Refresh();
                }
            }
        }

        private void SetRecipeImage(string recipeImageName)
        {
            string recipeImageFolder = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\"));
            recipeImageFolder = Path.Combine(recipeImageFolder, $"RecipeImages\\{recipeImageName}");

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(recipeImageFolder, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();

            RecImage.Source = bitmap;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var r = MessageBox.Show("Are you sure you want to delete this recipe?", "Delete Recipe", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;
            else
            {
                try
                {
                    recipe toDelete = Globals.dbContext.recipes
                        .Where(recipe => recipe.recipeId == selRecipe.recipeId)
                        .FirstOrDefault();
                    Globals.dbContext.recipes.Remove(toDelete);
                    Globals.dbContext.SaveChanges();
                    statusUpdate = "Recipe deleted";
                    this.DialogResult = false;
                    this.Close();
                }
                catch (SystemException ex)
                {
                    MessageBox.Show(this, "Error reading database\n" + ex.Message, "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!update)
            {
                update = true;
                TbxDescrip.Visibility = Visibility.Visible;
                TbxInstruc.Visibility = Visibility.Visible;
                TbxName.Visibility = Visibility.Visible;

                TbxDescrip.Text = selRecipe.description;
                TbxInstruc.Text = selRecipe.instructions;
                TbxName.Text = selRecipe.recipeName;

                txtbRecipeDescription.Visibility = Visibility.Hidden;
                txtbRecipeInstructions.Visibility = Visibility.Hidden;
                txtbRecipeName.Visibility = Visibility.Hidden;
            }
            else if (update)
            {
                var r = MessageBox.Show("Are you sure you want to updated this recipe?", "Update Recipe", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r != MessageBoxResult.Yes)
                {
                    return;
                }
                else
                {
                    try
                    {
                        recipe toUpdate = Globals.dbContext.recipes
                        .Where(recipe => recipe.recipeId == selRecipe.recipeId)
                        .FirstOrDefault();
                        toUpdate.recipeName = TbxName.Text;
                        toUpdate.description = TbxDescrip.Text;
                        toUpdate.instructions = TbxInstruc.Text;

                        Globals.dbContext.recipes.AddOrUpdate(toUpdate);

                        List<recipeingredient> clearRepIng = new List<recipeingredient>(Globals.dbContext.recipeingredients
                            .Where(recipeingredient => recipeingredient.recipeId == toUpdate.recipeId));
                        foreach (var i in clearRepIng)
                        {
                            Globals.dbContext.recipeingredients.Remove(i);
                        }
                        InsertIngredients();

                        int repInt = Globals.dbContext.recipes
                    .Where(recipe => recipe.recipeName == toUpdate.recipeName)
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

                        Globals.dbContext.SaveChanges();
                        statusUpdate = "Recipe Updated";
                        this.Close();
                    }
                    catch (SystemException ex)
                    {
                        MessageBox.Show(this, "Error reading database\n" + ex.Message, "Fatal error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
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

        private void dgRecipeView_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Check if right-click was on a valid DataGrid row
            var row = sender as DataGridRow;
            if (row == null || row.IsMouseOver)
            {
                ClickDelete.IsEnabled = false;
            }
            else if (row != null)
            {
                // Retrieve the clicked item
                var clickedItem = row.Item as CombinedIngredientData;
                if (clickedItem == null) return; // Ensure a valid item is clicked
            }

            // Display the context menu at the mouse pointer's position
            var contextMenu = dgRecipeView.ContextMenu;
            if (contextMenu != null)
            {
                contextMenu.PlacementTarget = sender as UIElement;
                contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
                contextMenu.IsOpen = true;
            }

            // Mark the event as handled
            e.Handled = true;
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
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

                dgRecipeView.Items.Refresh();
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Handle the Delete menu item click
            CombinedIngredientData toDelete = dgRecipeView.SelectedItem as CombinedIngredientData;
            if (toDelete == null)
            {
                MessageBox.Show("Something went wrong.");
                return;
            }
            combinedIngredients.Remove(toDelete);
            ingredient ingToDel = ingredients.Where(ingredient => ingredient.ingredientName == toDelete.Ingredient).FirstOrDefault();
            if (ingToDel != null)
            {
                ingredients.Remove(ingToDel);
            }
            dgRecipeView.Items.Refresh();


        }
    }
}
