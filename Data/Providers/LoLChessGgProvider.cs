using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace TFT_Overlay.Data.Providers
{
    public class LoLChessGgProvider : IDataProvider
    {
        public Dictionary<string, TftItem> Items { get; } = new Dictionary<string, TftItem>();
        public Dictionary<string, TftChampion> Champions { get; } = new Dictionary<string, TftChampion>();
        public Dictionary<string, TftClass> Classes { get; } = new Dictionary<string, TftClass>();
        public Dictionary<string, TftOrigin> Origins { get; } = new Dictionary<string, TftOrigin>();

        public LoLChessGgProvider()
        {
            // Query data from lolchess.gg
            // TODO: cache it and check cached data
            QueryData();
        }

        private void QueryData()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add ("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; TFT-Overlay " + Version.version + ")");
                try
                {
                    var context = BrowsingContext.New(Configuration.Default);
                    var parser = context.GetService<IHtmlParser>();
                    
                    // items: https://lolchess.gg/items -> Item List class=guide-items-table
                    var html = client.DownloadString("https://lolchess.gg/items");
                    var document = parser.ParseDocument(html);
                    foreach (var element in document.QuerySelectorAll(".guide-items-table tbody tr"))
                    {
                        var image = (IHtmlImageElement) element.QuerySelector("td img");
                        var name = image.AlternativeText;
                        TftItem item = new TftItem(
                            name.ToLower(),
                            name,
                            GetSource(image),
                            element.QuerySelector(".desc").TextContent.Trim()
                        );
                        
                        foreach (IHtmlImageElement ingredientImage in element.QuerySelectorAll(".combination img"))
                        {
                            var ingredientItem = Items[ingredientImage.AlternativeText.ToLower()];
                            if (ingredientItem != null)
                            {
                                item.Ingredients.Add(ingredientItem);
                                ingredientItem.Ingredient.Add(item);
                            }
                        }
                        Items.Add(item.Id, item);
                    }
                    
                    // synergies: https://lolchess.gg/synergies
                    html = client.DownloadString("https://lolchess.gg/synergies");
                    document = parser.ParseDocument(html);
                    foreach (var element in document.QuerySelectorAll(".guide-synergy-table .guide-synergy-table__container"))
                    {
                        // "Origins" or "Classes"
                        string type = element.QuerySelector(".guide-synergy-table__header").TextContent.Trim();
                        foreach (var entryElement in element.QuerySelectorAll(".guide-synergy-table__synergy"))
                        {
                            string id = entryElement.ClassName.Substring("guide-synergy-table__synergy guide-synergy-table__synergy--".Length);
                            var img = (IHtmlImageElement) entryElement.QuerySelector(".tft-hexagon img");
                            var descElement = entryElement.QuerySelector(".guide-synergy-table__synergy__desc");
                            string description = descElement != null ? descElement.TextContent.Trim() : "";
                            string effects = Trim(entryElement.QuerySelector(".guide-synergy-table__synergy__stats").TextContent);
                            switch (type)
                            {
                                case "Origins":
                                    Origins.Add(id, new TftOrigin(id, img.AlternativeText, GetSource(img), description, effects));
                                    break;
                                case "Classes":
                                    Classes.Add(id, new TftClass(id, img.AlternativeText, GetSource(img), description, effects));
                                    break;
                                default:
                                    Console.Error.WriteLine("Unknown synergy type " + type);
                                    break;
                            }
                        }
                    }
                    
                    // champions https://lolchess.gg/champions/anivia -> class=guide-champion-table
                    var link = "https://lolchess.gg/champions/aatrox";
                    html = client.DownloadString(link);
                    document = parser.ParseDocument(html);
                    foreach (IHtmlAnchorElement element in document.QuerySelectorAll(".guide-champion-list .guide-champion-list__content a.guide-champion-list__item"))
                    {
                        IHtmlDocument champDoc;
                        var champLink = element.Href;
                        if (!champLink.Equals(link))
                        {
                            champDoc = parser.ParseDocument(client.DownloadString(champLink));
                        }
                        else
                        {
                            champDoc = document;
                        }

                        var iconElement = (IHtmlImageElement) element.QuerySelector(".guide-champion-item img");
                        
                        var statsElement = champDoc.QuerySelectorAll(".guide-champion-detail__base-stats .guide-champion-detail__base-stat");
                        var spellElement = champDoc.QuerySelector(".guide-champion-detail__skill");
                        var spellImg = (IHtmlImageElement) spellElement.QuerySelector("img.guide-champion-detail__skill__icon");
                        
                        var synergies = new List<ITftSynergy>();

                        var detailStatsElement = champDoc.QuerySelectorAll(".guide-champion-detail__stats .guide-champion-detail__stats__row");
                        foreach (IHtmlImageElement originImage in detailStatsElement[1].QuerySelectorAll(".guide-champion-detail__stats__value img"))
                        {
                            synergies.Add(Origins[originImage.AlternativeText.ToLower()]);
                        }
                        foreach (IHtmlImageElement classImage in detailStatsElement[2].QuerySelectorAll(".guide-champion-detail__stats__value img"))
                        {
                            synergies.Add(Classes[classImage.AlternativeText.ToLower()]);
                        }   
                        
                        string id = champLink.Substring("https://lolchess.gg/champions/".Length);

                        double speed = 0.0;
                        int armor = 0;
                        int magicResistance = 20;
                        int range = 1;
                        int spellMana = 0;
                        try
                        {
                            speed = double.Parse(Trim(statsElement[4].QuerySelector(".guide-champion-detail__base-stat__value").TextContent));
                            armor = int.Parse(Trim(statsElement[5].QuerySelector(".guide-champion-detail__base-stat__value").TextContent));
                            magicResistance = int.Parse(Trim(statsElement[6].QuerySelector(".guide-champion-detail__base-stat__value").TextContent));
                            range = int.Parse(
                                ((IHtmlImageElement) statsElement[3].QuerySelector(".guide-champion-detail__base-stat__value img")).Source
                                .Substring("https://cdn.lolchess.gg/images/icon/ico-attack-distance-0".Length, 1));
                            spellMana = int.Parse(Trim(spellElement.QuerySelector("div.text-gray img+span").TextContent).Substring("Mana: ".Length));
                        }
                        catch (FormatException ex)
                        {
                            Console.Error.WriteLine(ex);
                            MessageBox.Show(ex.ToString(), "An error occured loading data", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        string name = champDoc.QuerySelector(".guide-champion-detail__name").TextContent;
                        string cost = element.QuerySelector(".guide-champion-item .cost").TextContent.Substring(1);
                        string health = TrimMulti(statsElement[0].QuerySelector(".guide-champion-detail__base-stat__value").TextContent);
                        string damage = TrimMulti(statsElement[1].QuerySelector(".guide-champion-detail__base-stat__value").TextContent);
                        string dps = TrimMulti(statsElement[2].QuerySelector(".guide-champion-detail__base-stat__value").TextContent);

                        var spellDescriptionElement = spellElement.QuerySelector("span.d-block.mt-1");
                        string spellDescription = spellDescriptionElement != null ? Trim(spellDescriptionElement.TextContent) : "";
                        
                        var spellEffectElement = spellElement.QuerySelector(".guide-champion-detail__skill__stats");
                        string spellEffect = spellEffectElement != null ? Trim(spellEffectElement.TextContent) : "";
                        
                        string spellType = Trim(spellElement.QuerySelector("div.text-gray span").TextContent);
                        
                        var spell = new Spell(spellImg.AlternativeText.ToLower(), spellImg.AlternativeText,
                            spellImg.Source, spellDescription, spellEffect, spellType, spellMana);
                        bool pbe = element.QuerySelector(".guide-champion-item.pbe") != null;
                        
                        var champion = new TftChampion(id, name, GetSource(iconElement), synergies, cost, health,
                            damage, dps, range, speed, armor, magicResistance, spell, pbe);
                        
                        Champions.Add(champion.Id, champion);
                        
                        foreach (var synergy in champion.Synergies)
                        {
                            synergy.Champions.Add(champion);
                        }
                        
                    }
                }
                catch (WebException ex)
                {
                    Console.Error.WriteLine(ex);
                    MessageBox.Show(ex.ToString(), "An error occured loading data", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string GetSource(IHtmlImageElement image)
        {
            if (image.Source.StartsWith("about:"))
            {
                return "https:" + image.Source.Substring("about:".Length);
            }

            return image.Source;
        }

        private string TrimMulti(string text)
        {
            var parts = Trim(text).Split('\n');
            return parts[0] + (parts.Length > 1 ? " " + parts[1] : "");
        }

        private string Trim(string text)
        { 
            string r = "";
            foreach (string s in text.Trim().Split('\n'))
            {
                if (s.Trim().Length >0)
                {
                    if (r.Length > 0)
                    {
                        r += "\n";
                    }
                    r += s.Trim();
                }
            }

            return r;
        }
    }
}