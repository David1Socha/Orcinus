using Godot;
using Newtonsoft.Json;
using Orcinus.Scripts.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts.Core
{
    public class DialogueHandler : CanvasLayer
    {
        private static DialogueHandler Instance;
        private RandomNumberGenerator _random;

        [Export(PropertyHint.MultilineText)]
        private string DialogueBank { get; set; }

        private Dialogue[] _dialogues;

        [Export]
        private readonly PackedScene DialoguePopupScene;

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            _random = new RandomNumberGenerator();
            _random.Randomize();

            _dialogues = JsonConvert.DeserializeObject<Dialogue[]>(DialogueBank);
        }

        public static Task PlayDialogue(int dialogueId, IDialogueSource dialogueSource)
        {
            // TODO pull dialogue based on ID (store in json??)
            var dialogue = Instance._dialogues.SingleOrDefault(d => d.Id == dialogueId);
            if (dialogue == null)
            {
                GD.PrintErr($"Dialogue with ID {dialogueId} was not found");
                return Task.CompletedTask;
            }

            return Instance.PlayDialogueInternal(dialogue, dialogueSource);
        }

        // this is pretty coupled to Orcinus, so doesn't really make sense in "Core" project. But it's less work this way and shouldn't really cause problems ¯\_(ツ)_/¯
        public static async Task PlayQuip(IDialogueSource dialogueSource)
        {
            await Instance.PlayQuipInternal(dialogueSource);
        }

        public async Task PlayQuipInternal(IDialogueSource dialogueSource)
        {
            var dialogue = _dialogues.Where(d => d.AssociatedOrca?.GetDescription() == dialogueSource.DialogueName).ToArray().PickRandomElement(_random);
            await PlayDialogueInternal(dialogue, dialogueSource);
        }

        public async Task PlayDialogueInternal(Dialogue dialogue, IDialogueSource dialogueSource)
        {
            GetTree().Paused = true;

            foreach (var line in dialogue.Lines)
            {
                await Instance.PlayDialogueLine(line, dialogueSource);
            }

            GetTree().Paused = false;
        }

        public static Task PlayDialogue(int dialogueId, IDictionary<string, IDialogueSource> dialogueSources)
        {
            return Instance.PlayDialogueInternal(dialogueId, dialogueSources);
        }

        private async Task PlayDialogueInternal(int dialogueId, IDictionary<string, IDialogueSource> dialogueSources)
        {
            GetTree().Paused = true;

            var dialogue = _dialogues.SingleOrDefault(d => d.Id == dialogueId);

            foreach (var line in dialogue.Lines)
            {
                var src = dialogueSources[line.Source];
                await Instance.PlayDialogueLine(line, src);
            }

            GetTree().Paused = false;
        }

        public async Task PlayDialogueLine(DialogueLine line, IDialogueSource src)
        {
            var popup = DialoguePopupScene.Instance<DialogueBox>();
            popup.Line = line;
            popup.DialoguePosition = src.DialogueScreenPosition;
            popup.SpeakerName = src.DialogueName;
            popup.PitchScale = src.PitchScale;
            popup.DialogueColor = src.DialogueColor ?? new Color(1f, 1f, 1f, 1f);
            this.AddChildDeferred(popup);

            await GDUtils.ToTask(ToSignal(popup, Signals.DialogueClosed));
            popup.QueueFree();
        }
    }
}
