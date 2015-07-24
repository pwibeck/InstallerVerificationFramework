namespace InstallerVerificationLibrary
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public static class CollectionTools
    {
        public static ICollection<T> CopyCollection<T>(ICollection<T> col)
        {
            if (col == null)
            {
                throw new ArgumentNullException("col");
            }

            var newCol = new Collection<T>();
            foreach (var item in col)
            {
                newCol.Add((T)item);
            }

            return newCol;
        }

        public static Collection<T> CloneCollection<T>(Collection<T> col) where T : ICloneable
        {
            if (col == null)
            {
                throw new ArgumentNullException("col");
            }

            var newCol = new Collection<T>();
            foreach (var item in col)
            {
                newCol.Add((T)item.Clone());
            }

            return newCol;
        }

        public static void MergeCollection<T>(ICollection<T> main, ICollection<T> add, bool avoidDuplicates)
        {
            if (main == null)
            {
                throw new ArgumentNullException("main");
            }

            if (add == null)
            {
                throw new ArgumentNullException("add");
            }

            foreach (var item in add)
            {
                if (avoidDuplicates)
                {                    
                    if (!main.Contains(item))
                    {
                        main.Add(item);
                    }                    
                }
                else
                {
                    main.Add(item);
                }
            }
        }
    }
}