using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WarOfWesteros
{
    static class Extensions
    {
        public static void ToConsole<T>(IEnumerable<T> input, string str)
        {
            Console.WriteLine("*** BEGIN " + str + " ***");
            foreach (T item in input)
            {
                Console.WriteLine(item.ToString());
            }
            Console.WriteLine("*** END " + str + " ***" );
            Console.ReadLine();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            XDocument doc = XDocument.Load("http://users.nik.uni-obuda.hu/prog3/_data/war_of_westeros.xml");

            // Q1 = Number of Houses participating in the war
            IEnumerable<string> q1 = doc.Descendants("house").Select(node => node.Value).Distinct();
            Console.WriteLine("NUMBER OF HOUSE: " + q1.Count());
            Extensions.ToConsole(q1, "Q1 - HOUSE");
            //q1.ToConsole("Q1 - House");

            // Q2 = list of ambushes
            string str = "ambush";
            IEnumerable<string> q2 = doc.Descendants("battle").Where(node => node.Element("type").Value == str)
                .Select(node => node.Element("name").Value);
            IEnumerable<string> q2b = from node in doc.Descendants("battle")
                                      where node.Element("type").Value == str
                                      select node.Element("name").Value;

            Extensions.ToConsole(q2, "Q2 - Ambushes");

            // Q3 = Number of battles with defender victories and any majorcapture
            IEnumerable<string> q3 = doc.Descendants("battle").Where(node => node.Element("outcome").Value == "defender" &&
                                     node.Element("majorcapture").Value.ToString() != "0").Select(node => node.Element("name").Value);
            IEnumerable<string> q3b = from battle in doc.Descendants("battle")
                                      where battle.Element("outcome").Value == "defender" &&
                                      (int)battle.Element("majorcapture") > 0
                                      select battle.Element("name").Value;
            Console.WriteLine("NUMBER OF BATTLES: " + q3b.Count());
            Extensions.ToConsole(q3, "Q3");

            // Q4 = Battles won by the Stark house
            var q4 = doc.Descendants("battle")
                .Where(x => x.Element(x.Element("outcome").Value).Descendants("house").Select(y=>y.Value).Contains("Stark"))
                .Select(x => x.Element("name").Value);

                
            var q4b = from node in doc.Descendants("battle")
                      where node.Element(node.Element("outcome").Value).Descendants("house").Select(x => x.Value).Contains("Stark")
                      select node.Element("name").Value;
            Extensions.ToConsole(q4, "Q4 - Stark Won Battles");


            // Q5 = Battles with more than 2 houses

            var q5b = from node in doc.Descendants("battle")
                     let attackerSize = node.Element("attacker").Elements("house").Count()
                     let defenderSize = node.Element("defender").Elements("house").Count()
                     let sumSize = attackerSize + defenderSize
                     where sumSize > 2
                     orderby sumSize descending
                     select new { Battle = node.Element("name").Value, Houses = sumSize, Region = node.Element("region").Value };

            Extensions.ToConsole(q5b, "Q5 - LargeBattles");

            // Q6 = Top 3 region based on number of battles
            var q6b = from node in doc.Descendants("battle")
                      group node by node.Element("region").Value into grp
                      let cnt = grp.Count()
                      orderby cnt descending
                      select new { Count = cnt, Region = grp.Key };

            Extensions.ToConsole(q6b.Take(3), "Q6 - Battles / Reigon");

            // Q7 = Most frequent region
            // ElementAt/First/Last/Single/xxxOrDefault
            Console.WriteLine(q6b.FirstOrDefault());
            Console.ReadLine();

            // Q8 = q5 join q4
            var q8 = from region in q6b
                     join battle in q5b on region.Region equals battle.Region
                     select new { Battle = battle.Battle, Region = region.Region, NumBattle = region.Count };

            // Q9 = Houses in descending order by battles won
            var q9b = from node in doc.Descendants("battle")
                      let winner = node.Element("outcome").Value
                      let winnerHouses = node.Element(winner).Elements("house").Select(x => x.Value)
                      from house in winnerHouses
                      group house by house into grp
                      orderby grp.Count() descending
                      select new { House = grp.Key, BattlesWon = grp.Count() };

            Extensions.ToConsole(q9b, "Q9 - Won by House");

            // Q10 = Battle With Largest Army
            int maxSize = doc.Descendants("size").Max(node => (int)node);
            var q10b = from node in doc.Descendants("size")
                       where (int)node == maxSize
                       select new { Battle = node.Parent.Parent.Element("name").Value, Size = maxSize, King = node.Parent.Element("king").Value };

            Extensions.ToConsole(q10b, "Q10 - Largest Army");

            // Q11 = commander with most number of attacks
            var q11b = from attackerNode in doc.Descendants("attacker")
                      from commander in attackerNode.Descendants("commander")
                      group commander by commander.Value into grp
                      orderby grp.Count() descending
                      select new { AttackerCommander = grp.Key, Count = grp.Count() };

            Extensions.ToConsole(q11b.Take(3), "Q11 - AttackerCommanders");

        }
    }
}
