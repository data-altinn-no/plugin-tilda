using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Dan.Plugin.Tilda.Utils
{
    public static class DummyData
    {
        public static readonly string[] Adverbs =
            {
                "dearly", "deceivingly", "tediously", "rather", "bashfully", "speedily", "wildly", "woefully", "yearly",
                "frantically", "however", "honestly", "kindheartedly", "greedily", "intensely", "far", "occasionally",
                "openly", "potentially", "correctly", "sternly", "quirkily", "annually", "deeply", "briskly",
                "probably", "continually", "wrongly", "crazily", "hardly", "needily", "rudely", "bravely", "initially",
                "actually", "well", "highly", "queasily", "officially", "unexpectedly", "madly", "almost",
                "reproachfully", "briefly", "hopelessly", "painfully", "nervously", "lazily", "roughly", "certainly",
                "limply", "bitterly", "cleverly", "frankly", "instead", "strictly", "elsewhere", "helpfully", "weekly",
                "therefore", "fairly", "urgently", "else", "politely", "warmly", "basically", "queerly", "moreover",
                "upwardly", "uselessly", "coaxingly", "ahead", "carelessly", "gracefully", "only", "then",
                "unnecessarily", "ever", "fortunately", "victoriously", "definitely", "foolishly", "oddly", "zealously",
                "unimpressively", "totally", "tenderly", "abnormally", "kissingly", "hungrily", "solidly", "rarely",
                "playfully", "currently", "necessarily", "terrifically", "worriedly", "tightly", "joyfully",
                "unnaturally"
            };

        public static readonly string[] Adjectives =
            {
                "limping", "psychotic", "crazy", "womanly", "acidic", "unequaled", "satisfying", "silent", "dry",
                "shivering", "boorish", "tart", "rampant", "snobbish", "hot", "roasted", "vacuous", "cloudy",
                "neighborly", "humdrum", "puzzled", "delicious", "madly", "cute", "extra-large", "utopian", "brainy",
                "tasteless", "safe", "aboard", "gifted", "boiling", "sparkling", "disagreeable", "careful", "glamorous",
                "rural", "tacit", "abandoned", "ruthless", "envious", "faint", "ancient", "empty", "naive", "infamous",
                "hypnotic", "feigned", "used", "cool", "vivacious", "nimble", "helpless", "alike", "possessive",
                "bright", "macabre", "excited", "childlike", "hard-to-find", "fat", "knowledgeable", "grey",
                "calculating", "shaky", "black-and-white", "unaccountable", "well-groomed", "curved", "quarrelsome",
                "bored", "scintillating", "tense", "abashed", "mere", "mighty", "plant", "ubiquitous", "silly",
                "symptomatic", "bewildered", "cumbersome", "tame", "courageous", "alive", "inconclusive", "profuse",
                "encouraging", "standing", "lonely", "jazzy", "conscious", "venomous", "bouncy", "acoustic", "hapless",
                "fine", "zonked", "giant", "shocking"
            };

        public static readonly string[] Nouns =
            {
                "scale", "flame", "idea", "tomato", "dog", "impulse", "uncle", "meat", "moon", "talk", "twist", "fruit",
                "walk", "nut", "house", "bird", "frog", "pig", "bush", "control", "rail", "drop", "snake", "club",
                "decision", "marble", "lamp", "dad", "dirt", "bait", "boundary", "badge", "engine", "train", "pizza",
                "doctor", "grandmother", "spring", "profit", "letter", "umbrella", "pickle", "tin", "wool", "tub",
                "plot", "book", "war", "dust", "unit", "tramp", "mark", "guide", "work", "dog", "porter", "grape",
                "rule", "rose", "sign", "dinosaur", "toe", "bed", "feeling", "force", "neck", "judge", "teeth", "jail",
                "truck", "bee", "purpose", "blood", "calculator", "bit", "cent", "support", "berry", "bead", "bra",
                "zephyr", "nest", "scent", "part", "rate", "spider", "hand", "tiger", "zoo", "route", "parcel",
                "reward", "spark", "train", "bone", "end", "food", "insurance", "yarn", "women"
            };

        public static readonly string[] Tlds = { "com", "net", "no", "org", "cloud" };

        public static readonly string[] Street1 =
            {
                "Gås", "Terne", "Ande", "Måke", "Rype", "Skarv", "Hauk", "Meis", "Bjørk", "Gran", "Hassel", "Rogne",
                "Hassel", "Furu", "Eik", "Eike", "Alme", "Kirsebær", "Asal", "Bøk"
            };

        public static readonly string[] Street2 = { "veien", "gaten", "svingen", "lia", "haugen", "alléen" };

        public static string GetRandomAddress(int digest)
        {
            return GetEntryFromDigest(digest, Street1) + GetEntryFromDigest(digest, Street2) + " " + Clamp(digest, 1, 200) + ", 0"
                   + Clamp(digest, 100, 999).ToString() + " Oslo";
        }

        public static string GenerateDummyAmount(int digest)
        {
            return GenerateDummyNumber(digest).ToString() + " NOK";
        }

        public static string GenerateDummaryUri(int digest)
        {
            return "https://" + GetDummySentence(digest) + "." + GetDummyTld(digest) + "/" + GetDummySentence(digest >> 1, "-");
        }

        public static string GenerateDummyString(int digest)
        {
            return GetDummySentence(digest, " ");
        }

        public static float GenerateDummyNumber(int digest)
        {
            float result = Clamp(digest, 0, 1000000);
            if (Clamp(digest, 0, 9) > 6)
            {
                result = result + ((float)Clamp(digest, 0, 100) / 100);
            }

            return result;
        }

        public static DateTime GenerateDummyDateTime(int digest)
        {
            DateTime dt = DateTime.Parse("2016-12-24");
            dt = dt.AddYears(Clamp(digest, -5, 2));
            dt = dt.AddMonths(Clamp(digest, -5, 5));
            dt = dt.AddDays(Clamp(digest, -10, 10));
            //dt = dt.AddHours(Clamp(digest, -10, 10));
            //dt = dt.AddMinutes(Clamp(digest, -30, 30));
            //dt = dt.AddSeconds(Clamp(digest, -30, 30));

            return dt;
        }

        public static bool GenerateDummyBoolean(int digest)
        {
            return Clamp(digest, 0, 9) > 4;
        }

        public static string GenerateDummyBase64(int digest)
        {
            var str = new StringBuilder();
            var repeats = Clamp(digest, 10, 30);
            for (var i = 0; i < repeats; i++)
            {
                str = str.Append(digest);
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str.ToString()));
        }

        public static string GetDummySentence(int digest, string spacer = "")
        {
            return GetEntryFromDigest(digest, Adverbs) + spacer + GetEntryFromDigest(digest, Adjectives) + spacer + GetEntryFromDigest(digest, Nouns);
        }

        public static string GetDummyWord(int digest)
        {
            return GetEntryFromDigest(digest, Adverbs) + GetEntryFromDigest(digest, Adjectives) + GetEntryFromDigest(digest, Nouns);
        }

        public static string GetDummyTld(int digest)
        {
            return GetEntryFromDigest(digest, Tlds);
        }

        public static object GenerateDummyComplexObject(int digest)
        {
            return new { SomeNumber = digest, SomeName = GetDummySentence(digest) };
        }

        public static int GetDigest(string seed)
        {
            using (MD5 md5 = MD5.Create())
            {
                return BitConverter.ToInt32(md5.ComputeHash(Encoding.UTF8.GetBytes(seed)), 0);
            }
        }

        public static string GetEntryFromDigest(int digest, string[] input)
        {
            return input[Clamp(digest, 0, input.Length)];
        }

        public static int Clamp(int num, int min, int max)
        {
            if (min == max)
            {
                return min;
            }

            return (Math.Abs(num) % (max - min)) + min;
        }
    }
}
