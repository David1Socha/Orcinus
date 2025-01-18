using Godot;

public class MenuButton : Button
{
    private HBoxContainer _hBox;
    private TextureRect _textureRect;
    private Label _label;
    private TextureRect _focusIndicator;
    private TextureRect _focusIndicatorAbove;
    private TextureRect ActiveFocusIndicator => ShowFocusIndicatorAbove ? _focusIndicatorAbove : _focusIndicator;
    [Export]
    private readonly float StartingScale = 1f;

    [Export]
    private readonly float PressedScaleReduction = .1f;

    [Export]
    private readonly Texture ButtonIcon;

    [Export]
    private readonly Texture ButtonToggledOnIcon;

    [Export]
    private readonly string ButtonText;

    [Export]
    private readonly bool ShowFocusIndicatorAbove = false;

    private float PressedScale { get { return StartingScale - PressedScaleReduction; } }

    private bool _isPressed;

    public override void _Ready()
    {
        base._Ready();
        _isPressed = false;

        _hBox = GetNode<HBoxContainer>("HBoxContainer");
        _textureRect = GetNode<TextureRect>("HBoxContainer/TextureRect");
        _textureRect.Texture = ButtonIcon;
        _label = GetNode<Label>("HBoxContainer/Label");
        _label.Text = ButtonText;
        _focusIndicator = GetNode<TextureRect>("HBoxContainer/FocusIndicator");
        _focusIndicatorAbove = GetNode<TextureRect>("FocusIndicatorAbove");


        if (ToggleMode)
        {
            OnToggled(Pressed);
        }

    }

    public void ButtonDown()
    {
        _isPressed = true;
        ShrinkButtonContents();
    }

    private void RestoreButtonContents()
    {

        _hBox.RectScale = new Vector2(StartingScale, StartingScale);
        _focusIndicatorAbove.RectScale = new Vector2(StartingScale, StartingScale);

        // for reasons I don't quite understand, centering the shrunk object seems to require twice as much anchor reduction on the top than on the bottom
        //      -- might be size flags / grow direction?
        _hBox.AnchorLeft -= PressedScaleReduction / 2f;
        _hBox.AnchorTop -= PressedScaleReduction;
        _focusIndicatorAbove.AnchorLeft -= PressedScaleReduction / 2f;
        _focusIndicatorAbove.AnchorTop -= PressedScaleReduction;
    }

    private void ShrinkButtonContents()
    {
        _hBox.RectScale = new Vector2(PressedScale, PressedScale);
        _focusIndicatorAbove.RectScale = new Vector2(PressedScale, PressedScale);

        // for reasons I don't quite understand, centering the shrunk object seems to require twice as much anchor reduction on the top than on the bottom
        _hBox.AnchorLeft += PressedScaleReduction / 2f;
        _hBox.AnchorTop += PressedScaleReduction;
        _focusIndicatorAbove.AnchorLeft += PressedScaleReduction / 2f;
        _focusIndicatorAbove.AnchorTop += PressedScaleReduction;
    }

    public void ButtonUp()
    {
        // Godot doesn't fire ButtonUp event when button is pressed, but focus changes to another button before it is released. Adding this check as a workaround
        if (_isPressed)
        {
            _isPressed = false;
            RestoreButtonContents();
        }
    }

    public void OnFocus()
    {
        ActiveFocusIndicator.Visible = true;
    }

    public void OnUnfocus()
    {
        if (_isPressed)
        {
            // Godot doesn't fire ButtonUp event when button is pressed, but focus changes to another button before it is released. Adding this check as a workaround
            _isPressed = false;
            RestoreButtonContents();
        }

        ActiveFocusIndicator.Visible = false;
    }

    public void DisableButton()
    {
        Disabled = true;
        Modulate = new Color(1f, 1f, 1f, .5f);
    }

    public void OnToggled(bool isToggled)
    {
        if (isToggled)
        {
            _textureRect.Texture = ButtonToggledOnIcon;
            _textureRect.Modulate = new Color(1f, 1f, 1f);
        }
        else
        {
            _textureRect.Modulate = new Color(.5f, .5f, .5f);
            _textureRect.Texture = ButtonIcon;
        }
    }
}
