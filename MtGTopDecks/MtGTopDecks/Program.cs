using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace MtGTopDecks
{
    class Program
    {
        static StreamReader GetFile(string url)
        {
            Console.WriteLine(url);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader data = new StreamReader(stream, Encoding.Default);
            return data;
        }

        static List<string> GetDecks(string url)
        {
            List<string> decks = new List<String>();
            using (StreamReader data = GetFile(url))
            {
                string result = data.ReadToEnd();
                // 구형 URL 샘플
                // http://archive.wizards.com/magic/magazine/article.aspx?x=mtg/daily/eventcoverage/gpkc13/welcome#1a`
                // http://archive.wizards.com/magic/mtgdailyeventcoveragegpkc13welcomex16.dek?x=mtg/daily/eventcoverage/gpkc13/welcome&decknum=16
                // http://archive.wizards.com/magic/mtgdailyeventcoveragegpkc13welcomex16.dek
                // 신형 URL 샘플
                // http://magic.wizards.com/en/events/coverage/2014WC
                // http://magic.wizards.com/en/decklist/deck-list-1bd00394057d1cab46174491613c6605.txt?n=PATRICK%20CHAPIN%20-%20TOP%204%2C%202014%20WORLD%20CHAMPIONSHIP
                // 현재는 구형 URL에만 대응 중.
                //string pattern = "href=\"[\\w\\\\/]*\\.dek";
                string pattern = "href=\"[\\w\\\\/]*\\.dek\\?x=[\\w\\\\/&;=]*\"";
                Regex regex = new Regex(pattern);
                foreach (Match match in regex.Matches(result))
                {
                    string deck = match.Value.Substring(6, match.Value.Length - 6 - 1);
                    deck = deck.Replace("&amp;", "&");
                    deck = @"http://archive.wizards.com" + deck;
                    //string deck = @"http://archive.wizards.com" + match.Value.Substring(6, match.Value.Length - 6 - 1);
                    decks.Add(deck);
                }
            }
            return decks;
        }

        static Dictionary<string, int> GetDeck(string url)
        {
            Dictionary<string, int> deck = new Dictionary<string, int>();

            using (StreamReader data = GetFile(url))
            {
                while (data.Peek() >= 0)
                {
                    string line = data.ReadLine();
                    try
                    {
                        int count = Convert.ToInt32(line.Substring(0, 1));
                        string card = line.Substring(2);
                        if (0 < count && count < 5 && card.Length > 0)
                        {
                            Console.WriteLine("{0} : {1}", card, count);
                            if (deck.ContainsKey(card))
                            {
                                deck[card] += count;
                            }
                            else
                            {
                                deck.Add(card, count);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
            return deck;
        }

        static void Main(string[] args)
        {
            SortedDictionary<string, int> cards = new SortedDictionary<string, int>();

            List<string> decks = GetDecks(args[0]);
            foreach (string url in decks)
            {
                Dictionary<string, int> deck = GetDeck(url);
                foreach (KeyValuePair<string, int> pair in deck)
                {
                    if (cards.ContainsKey(pair.Key))
                    {
                        cards[pair.Key] += pair.Value;
                    }
                    else
                    {
                        cards.Add(pair.Key, pair.Value);
                    }

                }
            }

            /*
            string searchDirectory = Directory.GetCurrentDirectory();
            if (args.Length > 0)
            {
                searchDirectory = args[0];
            }
            string[] files = Directory.GetFiles(searchDirectory, "*.txt");
            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    try
                    {
                        int count = Convert.ToInt32(line.Substring(0, 1));
                        string card = line.Substring(2);
                        if (0 < count && count < 5 && card.Length > 0)
                        {
                            if (cards.ContainsKey(card))
                            {
                                cards[card] += count;
                            }
                            else
                            {
                                cards.Add(card, count);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }
            */

            // 결과를 csv 파일로 출력
            using (StreamWriter file = new StreamWriter(@"cards.txt"))
            {
                foreach (KeyValuePair<string, int> pair in cards)
                {
                    file.WriteLine("{0}, {1}", pair.Key, pair.Value);
                }
            }
        }
    }
}