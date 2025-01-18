using Godot;
using Orcinus.Scripts.Core;
using Orcinus.Scripts.Models;
using Orcinus.Scripts.Models.Missions;
using static Orcinus.Scripts.Models.Constants;

public class MissionDisplay : Control
{
    private int _MissionIndex;
    [Export(PropertyHint.Range, "0,2")]
    public int MissionOffset;

    private TextureRect _IncompleteMissionIcon;
    private TextureRect _CompleteMissionIcon;
    private RichTextLabel _MissionDescription, _MissionProgress;
    private Mission Mission { get { return ProgressTracker.Progress.Missions[_MissionIndex]; } }

    public override void _Ready()
    {
        base._Ready();

        GlobalSignalBus.RegisterHandler(Signals.SessionSaved, this, nameof(OnSessionSaved));
        GlobalSignalBus.RegisterHandler(Signals.PauseTriggered, this, nameof(OnPause));
        GlobalSignalBus.RegisterHandler(Signals.ProgressLoaded, this, nameof(OnProgressLoaded));

        _IncompleteMissionIcon = GetNode<TextureRect>("IncompleteMissionIcon");
        _CompleteMissionIcon = GetNode<TextureRect>("CompleteMissionIcon");
        _MissionDescription = GetNode<RichTextLabel>("MissionDescription");
        _MissionProgress = GetNode<RichTextLabel>("MissionProgress");
    }

    public void OnProgressLoaded()
    {
        var missionLevel = ProgressTracker.Progress.MissionLevel;
        _MissionIndex = (missionLevel - 1) * ProgressTracker.ActiveMissionCount + MissionOffset;
        UpdateMissionInfoBasedOnStats();
        UpdateIconsBasedOnMissionCompletionStatus(shouldAnimateNewUnlocks: false);
    }


    public void OnPause()
    {
        UpdateMissionInfoBasedOnStats();
    }

    public void OnSessionSaved()
    {
        UpdateMissionInfoBasedOnStats();
        UpdateIconsBasedOnMissionCompletionStatus(shouldAnimateNewUnlocks: true);
    }

    public void UpdateMissionInfoBasedOnStats()
    {
        bool shouldAddCurrentProgressToDisplayedTotal = ProgressTracker.Progress.IsCurrentSessionActive || Mission.IsSingleSession;
        var addedProgress = shouldAddCurrentProgressToDisplayedTotal ? Mission.GetCurrentMissionProgress(ProgressTracker.Progress) : 0;
        var progress = Mission.MissionProgress + addedProgress;

        _MissionDescription.BbcodeText = $"[center]{Mission.MissionDescription}[/center]";
        if (Mission.MissionThreshold > 1)
        {
            if (progress < Mission.MissionThreshold && !Mission.IsUnlocked)
            {
                _MissionProgress.BbcodeText = $"[center][b]{progress} / {Mission.MissionThreshold}[/b][/center]";
            }
            else
            {
                _MissionProgress.BbcodeText = $"[center][b]{Mission.MissionThreshold}[/b][/center]";
            }
        }
        else
        {
            _MissionProgress.Visible = false;
        }
    }

    private void UpdateIconsBasedOnMissionCompletionStatus(bool shouldAnimateNewUnlocks)
    {
        if (Mission.MissionWasJustUnlocked && shouldAnimateNewUnlocks)
        {
            _CompleteMissionIcon.Material.AsShaderMaterial().SetShaderParam(ShaderParams.PercentVisible, 0f);
            var tween = CreateTween();
            tween.TweenInterval(.25f);
            tween.TweenProperty(_CompleteMissionIcon.Material.AsShaderMaterial(), ShaderParams.GetParamNodePath(ShaderParams.PercentVisible), 1f, 1f);
            tween.TweenProperty(_IncompleteMissionIcon, "visible", false, 0f);
            tween.TweenProperty(_CompleteMissionIcon, "rect_scale", new Vector2(1.1f, 1.1f), .15f);
            tween.TweenProperty(_CompleteMissionIcon, "rect_scale", new Vector2(1f, 1f), .1f);

        }
        else if (Mission.IsUnlocked)
        {
            _CompleteMissionIcon.Material.AsShaderMaterial().SetShaderParam(ShaderParams.PercentVisible, 1f);
        }
        else
        {
            _CompleteMissionIcon.Material.AsShaderMaterial().SetShaderParam(ShaderParams.PercentVisible, 0f);
        }
    }

}
