using Orcinus.Scripts.Models;

namespace Orcinus.Scripts.Core
{
    public class Dialogue
    {
        public int Id { get; set; }
        public DialogueLine[] Lines { get; set; }
        public OrcaEnum? AssociatedOrca { get; set; }
    }

    public class DialogueLine
    {
        public string Line { get; set; }
        public string Source { get; set; }
    }
}
