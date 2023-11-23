using CookieCookbook.App;
using CookieCookbook.DataAccess;
using CookieCookbook.FileAccess;
using CookieCookbook.Recipes;
using CookieCookbook.Recipes.Ingredients;


const FileFormat Format = FileFormat.Json;

IStringsRepository stringsRepository = Format == FileFormat.Json ? new StringsJsonRepository() : new StringsTextualRepository();
var ingredientsRegister = new IngredientRegister();
const string FileName = "recipes";
var fileMetadata = new FileMetadata(FileName, Format).ToPath();


var cookiesRecipeApp = new CookiesRecipesApp(new RecipesRepository(
    stringsRepository, ingredientsRegister),
    new RecipesConsoleUserInteraction(ingredientsRegister));
cookiesRecipeApp.Run(fileMetadata);

