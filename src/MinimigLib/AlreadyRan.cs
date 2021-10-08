using System.Collections.Generic;

namespace Minimig
{
    internal class AlreadyRan
    {
        public int Count => ByFilename.Count;
        public Dictionary<string, MigrationRow> ByFilename { get; } = new Dictionary<string, MigrationRow>();
        public Dictionary<string, MigrationRow> ByHash { get; } = new Dictionary<string, MigrationRow>();
        public MigrationRow Last { get; }

        /// <summary>
        /// Populate the properties needed to search migrations by filename or by hash
        /// </summary>
        /// <param name="rows"></param>
        internal AlreadyRan(IEnumerable<MigrationRow> rows)
        {
            MigrationRow last = null;
            foreach (var row in rows)
            {
                ByFilename[row.Filename] = row;
                ByHash[row.Hash] = row;
                last = row;
            }

            Last = last;
        }
    }
}
