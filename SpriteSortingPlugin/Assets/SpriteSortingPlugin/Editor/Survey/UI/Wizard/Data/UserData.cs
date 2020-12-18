using System;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
{
    [Serializable]
    public class UserData
    {
        public Guid id = Guid.NewGuid();
        public string mailAddress = "";
    }
}