using System;
using Godot;
using Orcinus.Scripts.Core;
using static Orcinus.Scripts.Models.Constants;

namespace Orcinus.Scripts
{
    public class ProgressDisplay : Control
    {
        private ProgressRow _firstOrcaRow, _firstBiomeRow, _firstPowerupRow, _firstHatRow;
        private ScrollContainer _orcaScroll, _biomeScroll, _powerupScroll, _hatScroll;
        private TabContainer _tabContainer;

        [Signal]
        public delegate void ProgressDisplayClosed();

        public override void _Ready()
        {
            _tabContainer = GetNode<TabContainer>("TabContainer");
            _firstOrcaRow = GetNode<ProgressRow>("TabContainer/Orcas/OrcasVbox/Sumi");
            _firstBiomeRow = GetNode<ProgressRow>("TabContainer/Biomes/Biomes/Coral");
            _firstPowerupRow = GetNode<ProgressRow>("TabContainer/Powerups/Powerups/HealthPack");
            _firstHatRow = GetNode<ProgressRow>("TabContainer/Hats/Hats/None");
            _orcaScroll = GetNode<ScrollContainer>("TabContainer/Orcas");
            _biomeScroll = GetNode<ScrollContainer>("TabContainer/Biomes");
            _hatScroll = GetNode<ScrollContainer>("TabContainer/Hats");
            _powerupScroll = GetNode<ScrollContainer>("TabContainer/Powerups");

            GlobalSignalBus.RegisterHandler(Signals.ProgressRowGainedFocus, this, nameof(OnProgressRowFocused));
            
            base._Ready();
        }

        public void OnProgressRowFocused(ProgressRow focusedRow) {
            var currentScrollContainer = GetActiveScrollContainer();
            GD.Print(currentScrollContainer.Name);
            if (currentScrollContainer != null) {
                var focusOffset = (int)focusedRow.RectPosition.y;
                GD.Print("FO: " + focusOffset);
                var scrollStart = currentScrollContainer.RectPosition.y + currentScrollContainer.ScrollVertical;
                GD.Print("SS: " + scrollStart);
                var scrollBottom = scrollStart + currentScrollContainer.RectSize.y - focusedRow.RectSize.y;
                GD.Print("SB: " + scrollBottom);
                if (focusOffset < scrollStart || focusOffset >= scrollBottom) {
                    GD.Print("adjusting scrollvertical to offset");
                    currentScrollContainer.ScrollVertical = focusOffset;
                }
            }
            
        }

        public ScrollContainer GetActiveScrollContainer() {
            if (_tabContainer.CurrentTab == 0) {
                return _orcaScroll;
            } else if (_tabContainer.CurrentTab == 1) {
                return _biomeScroll;
            } else if (_tabContainer.CurrentTab == 2) {
                return _powerupScroll;
            } else if (_tabContainer.CurrentTab == 4) {
                return _hatScroll;
            } else {
                return null;
            }
        }

        public void OnCloseButtonPressed()
        {
            EmitSignal(Signals.ProgressDisplayClosed);
            Visible = false;
        }

        public override void _Input(InputEvent @event)
        {
            if (Visible) {
                if (@event.IsActionPressed("ui_cancel")) {
                    OnCloseButtonPressed();
                } else if (@event.IsActionPressed("ui_left")) {
                    PageTabsLeft();
                } else if (@event.IsActionPressed("ui_right")) {
                    PageTabsRight();
                }
            }
            
            base._Input(@event);
        }

        public void PageTabsLeft() {
            if (_tabContainer.CurrentTab != 0) {
                _tabContainer.CurrentTab--;
            }
            else {
                _tabContainer.CurrentTab = _tabContainer.GetTabCount() - 1;
            }
            FocusOnFirstRowInCurrentTab();
        }

        public void PageTabsRight() {
            if (_tabContainer.CurrentTab != _tabContainer.GetTabCount() - 1) {
                _tabContainer.CurrentTab++;
            }
            else {
                _tabContainer.CurrentTab = 0;
            }
            FocusOnFirstRowInCurrentTab();
        }

        public void FocusOnFirstRowInCurrentTab() {
            switch (_tabContainer.CurrentTab) {
                case 0: // orca tab
                    _firstOrcaRow.GrabFocusForRow();
                    break;
                case 1: // biome tab
                    _firstBiomeRow.GrabFocusForRow();
                    break;
                case 2: // powerup tab
                    _firstPowerupRow.GrabFocusForRow();
                    break;
                case 4: // hat tab
                    _firstHatRow.GrabFocusForRow();
                    break;
                case 3:
                default:
                    break;
            }
        }

        public void OnVisibilityChanged() {
            if (Visible) {
                FocusOnFirstRowInCurrentTab();
            }
        }
    }
}
