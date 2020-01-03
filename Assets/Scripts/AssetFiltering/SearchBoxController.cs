using UnityEngine;
using UnityEngine.UI;


public class SearchBoxController : MonoBehaviour {

    public AssetFilterManager filterManager;
    public Text clearSearchButtonText;
    public InputField searchField;

    public void TextChanged()
    {
        clearSearchButtonText.text = (searchField.text == string.Empty || searchField.text == "") ? "" : "";
        if (string.IsNullOrEmpty(searchField.text))
            filterManager.ClearFilters();
        else if (searchField.text == "Favourites")
            filterManager.FilterFavourites();
        else
        {
            filterManager.FilterBySearch(searchField.text);
        }
    }

    public void ClearButtonPressed()
    {
        if (searchField.text != string.Empty || searchField.text != "")
            searchField.text = string.Empty;
    }

    public void SetSearchText(Text text)
    {
        searchField.text = text.text;
    }
}
