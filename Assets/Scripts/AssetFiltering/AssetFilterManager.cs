using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssetFilterManager : MonoBehaviour {

    public InputField searchField;

	public List<string> GetAllChildsTags()
    {
        List<string> tags = new List<string>();
        foreach(Transform transform in this.transform)
        {
            CatalogueItemThumnail_Audio audio = transform.GetComponent<CatalogueItemThumnail_Audio>();
            if (audio != null)
            {
                foreach (string tag in audio.GetTags)
                {
                    tags.Add(tag);
                }
                    
            }
            CatalogueItemThumnail_Font font = transform.GetComponent<CatalogueItemThumnail_Font>();
            if (font != null)
            {
                foreach (string tag in font.GetTags)
                {
                    tags.Add(tag);
                }

            }
            CatalogueItemThumnail_CodeSnip code = transform.GetComponent<CatalogueItemThumnail_CodeSnip>();
            if (code != null)
            {
                foreach (string tag in code.GetTags)
                {
                    tags.Add(tag);
                }

            }
            CatalogueItemThumnail_Tile tile = transform.GetComponent<CatalogueItemThumnail_Tile>();
            if (tile != null)
            {
                foreach (string tag in tile.GetTags)
                {
                    tags.Add(tag);
                }

            }
            CatalogueItemThumnail_Skybox skybox = transform.GetComponent<CatalogueItemThumnail_Skybox>();
            if (skybox != null)
            {
                foreach (string tag in skybox.GetTags)
                {
                    tags.Add(tag);
                }
            }
            CatalogueItemThumnail_Graphic graphic = transform.GetComponent<CatalogueItemThumnail_Graphic>();
            if (graphic != null)
            {
                foreach (string tag in graphic.GetTags)
                {
                    tags.Add(tag);
                }
            }
            CatalogueItemThumnail_Model model = transform.GetComponent<CatalogueItemThumnail_Model>();
            if (model != null)
            {
                foreach (string tag in model.GetTags)
                {
                    tags.Add(tag);
                }
            }
        }
        return tags;
    }

    public void FilterBySearch(string search)
    {
        foreach (Transform trans in this.transform)
            trans.gameObject.SetActive(false);
        foreach (Transform transform in this.transform)
        {
            CatalogueItemThumnail_Audio audio = transform.GetComponent<CatalogueItemThumnail_Audio>();
            if (audio != null)
            {
                if (search.Contains("#"))
                {
                    string tagSearch = search.Replace("#", "");
                    foreach (string tag in audio.GetTags)
                        if (tag.ToUpper() == tagSearch.ToUpper())
                            audio.gameObject.SetActive(true);
                }
                else
                {
                    if (audio.lable.text.ToUpper().Contains(search.ToUpper()))
                        audio.gameObject.SetActive(true);
                }
            }
            CatalogueItemThumnail_Font font = transform.GetComponent<CatalogueItemThumnail_Font>();
            if (font != null)
            {
                if (search.Contains("#"))
                {
                    string tagSearch = search.Replace("#", "");
                    foreach (string tag in font.GetTags)
                        if (tag.ToUpper() == tagSearch.ToUpper())
                            font.gameObject.SetActive(true);
                }
                else
                {
                    if (font.lable.text.ToUpper().Contains(search.ToUpper()))
                        font.gameObject.SetActive(true);
                }
            }
            CatalogueItemThumnail_CodeSnip code = transform.GetComponent<CatalogueItemThumnail_CodeSnip>();
            if (code != null)
            {
                if (search.Contains("#"))
                {
                    string tagSearch = search.Replace("#", "");
                    foreach (string tag in code.GetTags)
                        if (tag.ToUpper() == tagSearch.ToUpper())
                            code.gameObject.SetActive(true);
                }
                else
                {
                    if (code.lable.text.ToUpper().Contains(search.ToUpper()))
                        code.gameObject.SetActive(true);
                }
            }
            CatalogueItemThumnail_Tile tile = transform.GetComponent<CatalogueItemThumnail_Tile>();
            if (tile != null)
            {
                if (search.Contains("#"))
                {
                    string tagSearch = search.Replace("#", "");
                    foreach (string tag in tile.GetTags)
                        if (tag.ToUpper() == tagSearch.ToUpper())
                            tile.gameObject.SetActive(true);
                }
                else
                {
                    if (tile.lable.text.ToUpper().Contains(search.ToUpper()))
                        tile.gameObject.SetActive(true);
                }
            }
            CatalogueItemThumnail_Skybox skybox = transform.GetComponent<CatalogueItemThumnail_Skybox>();
            if (skybox != null)
            {
                if (search.Contains("#"))
                {
                    string tagSearch = search.Replace("#", "");
                    foreach (string tag in skybox.GetTags)
                        if (tag.ToUpper() == tagSearch.ToUpper())
                            skybox.gameObject.SetActive(true);
                }
                else
                {
                    if (skybox.lable.text.ToUpper().Contains(search.ToUpper()))
                        skybox.gameObject.SetActive(true);
                }
            }
            CatalogueItemThumnail_Graphic graphic = transform.GetComponent<CatalogueItemThumnail_Graphic>();
            if (graphic != null)
            {
                if (search.Contains("#"))
                {
                    string tagSearch = search.Replace("#", "");
                    foreach (string tag in graphic.GetTags)
                        if (tag.ToUpper() == tagSearch.ToUpper())
                            graphic.gameObject.SetActive(true);
                }
                else
                {
                    if (graphic.lable.text.ToUpper().Contains(search.ToUpper()))
                        graphic.gameObject.SetActive(true);
                }
            }
            CatalogueItemThumnail_Model model = transform.GetComponent<CatalogueItemThumnail_Model>();
            if (model != null)
            {
                if (search.Contains("#"))
                {
                    string tagSearch = search.Replace("#", "");
                    foreach (string tag in model.GetTags)
                        if (tag.ToUpper() == tagSearch.ToUpper())
                            model.gameObject.SetActive(true);
                }
                else
                {
                    if (model.lable.text.ToUpper().Contains(search.ToUpper()))
                        model.gameObject.SetActive(true);
                }
            }
        }
    }

    public void FilterFavourites()
    {
        foreach (Transform trans in this.transform)
            trans.gameObject.SetActive(false);
        foreach (Transform transform in this.transform)
        {
            CatalogueItemThumnail_Audio audio = transform.GetComponent<CatalogueItemThumnail_Audio>();
            if (audio != null)
            {
                if (audio.favourite)
                    audio.gameObject.SetActive(true);
            }
            CatalogueItemThumnail_Font font = transform.GetComponent<CatalogueItemThumnail_Font>();
            if (font != null)
            {
                if (font.favourite)
                    font.gameObject.SetActive(true);
            }
            CatalogueItemThumnail_CodeSnip code = transform.GetComponent<CatalogueItemThumnail_CodeSnip>();
            if (code != null)
            {
                if (code.favourite)
                    code.gameObject.SetActive(true);
            }
            CatalogueItemThumnail_Tile tile = transform.GetComponent<CatalogueItemThumnail_Tile>();
            if (tile != null)
            {
                if (tile.favourite)
                    tile.gameObject.SetActive(true);
            }
            CatalogueItemThumnail_Skybox skybox = transform.GetComponent<CatalogueItemThumnail_Skybox>();
            if (skybox != null)
            {
                if (skybox.favourite)
                    skybox.gameObject.SetActive(true);
            }
            CatalogueItemThumnail_Graphic graphic = transform.GetComponent<CatalogueItemThumnail_Graphic>();
            if (graphic != null)
            {
                if (graphic.favourite)
                    graphic.gameObject.SetActive(true);
            }
            CatalogueItemThumnail_Model model = transform.GetComponent<CatalogueItemThumnail_Model>();
            if (model != null)
            {
                if (model.favourite)
                    model.gameObject.SetActive(true);
            }
        }
    }

    public void FilterByCategory(string filter)
    {
        searchField.text = null;
        foreach (Transform trans in this.transform)
            trans.gameObject.SetActive(false);
        if (filter == "All")
        {
            ClearFilters();
            return;
        }
        string[] split = filter.Split(';');
        int subCategory = int.Parse(split[1]);
        CatalogueItemDetail.ItemTypes type = (CatalogueItemDetail.ItemTypes)int.Parse(split[0]);
        switch (type)
        {
            case CatalogueItemDetail.ItemTypes.Model: //1
                foreach (Transform transform in this.transform)
                {
                    CatalogueItemThumnail_Model thumnail = transform.GetComponent<CatalogueItemThumnail_Model>();
                    if (thumnail != null)
                        if (subCategory == thumnail.GetSubCategory || subCategory == 0)
                            thumnail.gameObject.SetActive(true);
                }
                break;
            case CatalogueItemDetail.ItemTypes.Audio: //2
                foreach (Transform transform in this.transform)
                {
                    CatalogueItemThumnail_Audio thumnail = transform.GetComponent<CatalogueItemThumnail_Audio>();
                    if (thumnail != null)
                        if (subCategory == thumnail.GetSubCategory || subCategory == 0)
                            thumnail.gameObject.SetActive(true);
                }
                break;
            case CatalogueItemDetail.ItemTypes.Graphic: //3
                foreach (Transform transform in this.transform)
                {
                    CatalogueItemThumnail_Graphic thumnail = transform.GetComponent<CatalogueItemThumnail_Graphic>();
                    if (thumnail != null)
                        if (subCategory == thumnail.GetSubCategory || subCategory == 0)
                            thumnail.gameObject.SetActive(true);
                }
                break;
            case CatalogueItemDetail.ItemTypes.Tile: //4
                foreach (Transform transform in this.transform)
                {
                    CatalogueItemThumnail_Tile thumnail = transform.GetComponent<CatalogueItemThumnail_Tile>();
                    if (thumnail != null)
                        if (subCategory == thumnail.GetSubCategory || subCategory == 0)
                            thumnail.gameObject.SetActive(true);
                }
                break;
            case CatalogueItemDetail.ItemTypes.Animation: //5
                //To Do
                break;
            case CatalogueItemDetail.ItemTypes.Skybox: //6
                foreach (Transform transform in this.transform)
                {
                    CatalogueItemThumnail_Skybox thumnail = transform.GetComponent<CatalogueItemThumnail_Skybox>();
                    if (thumnail != null)
                        if (subCategory == thumnail.GetSubCategory || subCategory == 0)
                            thumnail.gameObject.SetActive(true);
                }
                break;
            case CatalogueItemDetail.ItemTypes.Font: //7
                foreach (Transform transform in this.transform)
                {
                    CatalogueItemThumnail_Font thumnail = transform.GetComponent<CatalogueItemThumnail_Font>();
                    if (thumnail != null)
                        if (subCategory == thumnail.GetSubCategory || subCategory == 0)
                            thumnail.gameObject.SetActive(true);
                }
                break;
            case CatalogueItemDetail.ItemTypes.CodeSnip: //8
                foreach (Transform transform in this.transform)
                {
                    CatalogueItemThumnail_CodeSnip thumnail = transform.GetComponent<CatalogueItemThumnail_CodeSnip>();
                    if (thumnail != null)
                        if (subCategory == thumnail.GetSubCategory || subCategory == 0)
                            thumnail.gameObject.SetActive(true);
                }
                break;
        }
}

    public void ClearFilters()
    {
        foreach (Transform trans in this.transform)
            trans.gameObject.SetActive(true);
    }
}
