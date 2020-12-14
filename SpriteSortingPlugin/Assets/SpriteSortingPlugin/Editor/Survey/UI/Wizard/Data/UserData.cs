using System;

namespace SpriteSortingPlugin.Survey.UI.Wizard.Data
{
    [Serializable]
    public class UserData
    {
        public Guid userGuid=Guid.NewGuid();
        public string mailAddress = "";
    }
}
