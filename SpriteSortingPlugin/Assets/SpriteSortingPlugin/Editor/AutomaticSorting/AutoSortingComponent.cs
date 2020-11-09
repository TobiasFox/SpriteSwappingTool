using System.Collections.Generic;

namespace SpriteSortingPlugin.AutomaticSorting
{
    public class AutoSortingComponent : SortingComponent
    {
        private static int globalID = 0;

        public readonly int id;

        public int sortingOrder;
        public AutoSortingComponent containedByAutoSortingComponent;

        private List<SortingComponent> overlappingSortingComponents;

        public static void ResetID()
        {
            globalID = 0;
        }

        public AutoSortingComponent(SortingComponent sortingComponent) : base(sortingComponent.spriteRenderer,
            sortingComponent.outmostSortingGroup)
        {
            id = globalID++;
        }

        public void AddOverlappingSortingComponent(SortingComponent sortingComponent)
        {
            if (overlappingSortingComponents == null)
            {
                overlappingSortingComponents = new List<SortingComponent>();
            }

            overlappingSortingComponents.Add(sortingComponent);
        }

        public bool IsOverlapping(AutoSortingComponent autoSortingComponent)
        {
            if (overlappingSortingComponents == null)
            {
                return false;
            }

            foreach (var overlappingSortingComponent in overlappingSortingComponents)
            {
                if (overlappingSortingComponent.Equals(autoSortingComponent))
                {
                    return true;
                }
            }

            return false;
        }
    }
}