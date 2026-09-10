/* ==========================================================================
   Planorahub — UI behaviour
   --------------------------------------------------------------------------
   Vanilla JS, no dependencies, one file shared by every page.

   Each module is optional: it initialises only when its markup hooks
   (data-* attributes) are present, so adding a new page costs nothing.

   1 . Password visibility toggle
   2 . Password strength + requirement checklist
   3 . Timed redirect (success screen)
   4 . Sidebar — desktop collapse to rail, mobile off-canvas drawer
   5 . View switch — list / grid
   6 . Filter tabs
   7 . Switches
   8 . Repeatable rows — clones the last row of a list (halls, venues)
   9 . Conditional disclosure
   10. Choice cards — event type
   11. Segmented controls
   12. Seating mode — scoped to a single ticket tier
   13. Accordions
   14. Pressed-option groups (publish timing, charge model, ...)
   15. CSV member-list upload
   16. Modal dialogs
   17. Status filtering
   18. Settings section navigation
   19. Two-stage publish dialog
   20. City -> timezone, applied automatically
   21. Inline row edit — agenda builder
   24. Event-type feature gating
   25. Numeric steppers
   26. Hub date/slot scoping
   27. Seat picker
   28. Availability toggles — menu items in the hub
   29. Registration-form preview
   30. Automatic session / slot names
   31. Cover image preview
   32. Bullet-aware textareas
   33. Dynamic wizard step numbering
   34. Session filtering inside the ticket-access picker
   35. "This ticket includes" summary
   36. Button feedback, and a floor under every button
   37. Ticket visibility toggle
   38. RSVP tab lock
   39. One-click preset questions
   40. Broadcast templates
   41. Description editor tools
   42. Brand colour presets
   43. Carrying the RSVP answer through to Event Hub
   44. Role-based access
   45. Pause and resume sales on a menu item
   46. Checking a guest in from the guests table
   47. Speaker directory
   48. Attaching speakers to a session
   49. Reusable form templates
   50. Pre-publish system check
   51. Broadcast audience builder
   52. Branding drives the event page preview
   53. Status-aware row action menus
   54. Cancellation scope and the refund it triggers
   55. Vendors, entry staff and their PINs
   56. Table search and column filter
   57. Food order stages
   58. Adding and editing a menu item
   59. Registration gate + submissions
   60. Door scanner and its verdict
   61. Communication credit packs
   62. Record picker (refund receipts)
   63. Seat layout jump
   64. Bank account lookup
   65. Buy-credits jump
   66. Ticketed vs non-ticketed gating
   67. Add capacity to a published event
   68. RSVP guest list
   69. Invitation recipients
   70. Invitation rich text
   71. Revoke an invitation
   72. Seat assignment before ticket delivery
   73. Check-in list, search and seating layout
   74. Payout account and bank verification
   75. Hub add-ons and promo codes
   76. Find-a-guest shortcut
   77. What is included with a ticket
   78. Line breaks become bullets
   79. Reuse branding from a previous event
   80. Payout ledger scoping
   ========================================================================== */

(function () {
    'use strict';

    /* ------------------------------------------------------------------------
       Helpers
       ------------------------------------------------------------------------ */

    function all(selector, scope) {
        return Array.prototype.slice.call((scope || document).querySelectorAll(selector));
    }

    var MOBILE_QUERY = '(max-width: 1024px)';

    function isCompact() {
        return window.matchMedia(MOBILE_QUERY).matches;
    }

    /* ------------------------------------------------------------------------
       1. Password visibility toggle
       ------------------------------------------------------------------------ */

    function initPasswordToggles() {
        all('[data-password-toggle]').forEach(function (toggle) {
            var input = document.getElementById(toggle.getAttribute('aria-controls'));
            if (!input) return;

            var label = toggle.querySelector('.u-visually-hidden');

            toggle.addEventListener('click', function () {
                var isVisible = toggle.getAttribute('aria-pressed') === 'true';

                toggle.setAttribute('aria-pressed', String(!isVisible));
                input.type = isVisible ? 'password' : 'text';

                if (label) {
                    label.textContent = isVisible ? 'Show password' : 'Hide password';
                }
            });
        });
    }

    /* ------------------------------------------------------------------------
       2. Password strength + requirement checklist
       ------------------------------------------------------------------------ */

    var RULES = {
        length: function (value) { return value.length >= 8; },
        uppercase: function (value) { return /[A-Z]/.test(value); },
        number: function (value) { return /\d/.test(value); },
        special: function (value) { return /[^A-Za-z0-9]/.test(value); }
    };

    var STRENGTH_LABELS = [
        '',
        'Weak password',
        'Fair password',
        'Good password',
        'Strong password'
    ];

    function initPasswordStrength() {
        var form = document.querySelector('[data-password-form]');
        if (!form) return;

        var input = form.querySelector('[data-password-input]');
        if (!input) return;

        var confirm = form.querySelector('[data-password-confirm]');
        var strength = form.querySelector('[data-strength]');
        var segments = all('[data-strength-segment]', form);
        var strengthLabel = form.querySelector('[data-strength-label]');
        var ruleItems = all('[data-rule]', form);

        function evaluate() {
            var value = input.value;
            var score = 0;

            ruleItems.forEach(function (item) {
                var rule = RULES[item.getAttribute('data-rule')];
                var isMet = Boolean(rule) && rule(value);

                item.classList.toggle('is-met', isMet);
                if (isMet) score += 1;
            });

            if (strength) {
                strength.hidden = value.length === 0;
                strength.setAttribute('data-level', String(score));
            }

            segments.forEach(function (segment, index) {
                segment.classList.toggle('is-filled', index < score);
            });

            if (strengthLabel) {
                strengthLabel.textContent = STRENGTH_LABELS[score];
            }

            return score;
        }

        input.addEventListener('input', evaluate);

        form.addEventListener('submit', function (event) {
            if (evaluate() < 4) {
                event.preventDefault();
                input.focus();
                return;
            }

            if (confirm && confirm.value !== input.value) {
                event.preventDefault();
                confirm.focus();
                confirm.select();
            }
        });

        evaluate();
    }

    /* ------------------------------------------------------------------------
       3. Timed redirect (success screen)
       ------------------------------------------------------------------------ */

    function initRedirect() {
        var target = document.querySelector('[data-redirect]');
        if (!target) return;

        var url = target.getAttribute('data-redirect-to');
        var delay = parseInt(target.getAttribute('data-redirect-delay'), 10);

        if (!url || isNaN(delay)) return;

        window.setTimeout(function () {
            window.location.assign(url);
        }, delay);
    }

    /* ------------------------------------------------------------------------
       4. Sidebar — desktop collapse to rail, mobile off-canvas drawer
       ------------------------------------------------------------------------ */

    function initSidebar() {
        var sidebar = document.querySelector('[data-sidebar]');
        if (!sidebar) return;

        var railClass = sidebar.getAttribute('data-rail-class') || 'sidenav--rail';
        var collapse = document.querySelector('[data-sidebar-collapse]');
        var burger = document.querySelector('[data-sidebar-open]');
        var toggle = document.querySelector('[data-sidebar-toggle]');
        var backdrop = document.querySelector('[data-backdrop]');

        function closeDrawer() {
            sidebar.classList.remove('is-open');
            if (backdrop) backdrop.classList.remove('is-open');
            if (burger) burger.setAttribute('aria-expanded', 'false');
        }

        function openDrawer() {
            sidebar.classList.add('is-open');
            if (backdrop) backdrop.classList.add('is-open');
            if (burger) burger.setAttribute('aria-expanded', 'true');
        }

        if (collapse) {
            collapse.addEventListener('click', function () {
                var isRail = sidebar.classList.toggle(railClass);
                collapse.setAttribute('aria-expanded', String(!isRail));
            });
        }

        if (burger) {
            burger.addEventListener('click', function () {
                if (sidebar.classList.contains('is-open')) {
                    closeDrawer();
                } else {
                    openDrawer();
                }
            });
        }

        // Topbar menu button: opens the drawer on narrow screens, collapses the
        // sidebar to its icon rail on wide ones.
        if (toggle) {
            toggle.addEventListener('click', function () {
                if (isCompact()) {
                    if (sidebar.classList.contains('is-open')) {
                        closeDrawer();
                    } else {
                        openDrawer();
                    }
                    return;
                }

                var isRail = sidebar.classList.toggle(railClass);
                toggle.setAttribute('aria-expanded', String(!isRail));
            });
        }

        if (backdrop) backdrop.addEventListener('click', closeDrawer);

        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') closeDrawer();
        });

        // Leaving compact widths should never leave a stale drawer open.
        window.addEventListener('resize', function () {
            if (!isCompact()) closeDrawer();
        });
    }

    /* ------------------------------------------------------------------------
       5. View switch — list / grid
       ------------------------------------------------------------------------ */

    function initViewSwitch() {
        all('[data-viewswitch]').forEach(function (group) {
            var buttons = all('[data-view]', group);

            buttons.forEach(function (button) {
                button.addEventListener('click', function () {
                    var view = button.getAttribute('data-view');

                    buttons.forEach(function (other) {
                        // Respect whichever state attribute the markup already uses, so
                        // one module drives both toggle buttons and ARIA tabs.
                        var attr = other.hasAttribute('aria-selected') ? 'aria-selected' : 'aria-pressed';
                        other.setAttribute(attr, String(other === button));
                    });

                    all('[data-view-panel]', group.getAttribute('data-viewswitch-scope')
                        ? document.querySelector(group.getAttribute('data-viewswitch-scope'))
                        : document).forEach(function (panel) {
                            panel.hidden = panel.getAttribute('data-view-panel') !== view;
                        });
                });
            });
        });
    }

    /* ------------------------------------------------------------------------
       6. Filter tabs
       ------------------------------------------------------------------------ */

    function initTabs() {
        all('[data-tabs]').forEach(function (group) {
            var tabs = all('[role="tab"]', group);

            tabs.forEach(function (tab) {
                tab.addEventListener('click', function () {
                    tabs.forEach(function (other) {
                        other.setAttribute('aria-selected', String(other === tab));
                    });
                });
            });
        });
    }

    /* ------------------------------------------------------------------------
       7. Switches
       ------------------------------------------------------------------------ */

    function initSwitches() {
        all('[data-switch]').forEach(function (button) {
            button.addEventListener('click', function () {
                var isOn = button.getAttribute('aria-checked') === 'true';
                button.setAttribute('aria-checked', String(!isOn));
            });
        });
    }

    /* ------------------------------------------------------------------------
       8. Repeatable rows — clones the last row of a list (halls, venues)
       ------------------------------------------------------------------------ */

    function initRepeaters() {
        all('[data-repeat-add]').forEach(function (button) {
            var list = document.getElementById(button.getAttribute('data-repeat-add'));
            if (!list) return;

            button.addEventListener('click', function () {
                var rows = list.children;
                if (!rows.length) return;

                var clone = rows[rows.length - 1].cloneNode(true);

                all('input, textarea', clone).forEach(function (input) {
                    if (input.type !== 'checkbox' && input.type !== 'radio') input.value = '';
                });

                rekey(clone);
                list.appendChild(clone);

                var firstInput = clone.querySelector('input, select');
                if (firstInput) firstInput.focus();

                document.dispatchEvent(new CustomEvent('planora:rowsChanged'));
            });
        });

        // Row removal is delegated so cloned rows work without re-binding.
        document.addEventListener('click', function (event) {
            var trigger = event.target.closest && event.target.closest('[data-repeat-remove]');
            if (!trigger) return;

            var row = trigger.closest('[data-repeat-row]');
            if (!row || !row.parentNode) return;

            // Refusing to remove the last one is right — doing it silently is not.
            // A button that looks live and does nothing reads as broken.
            if (row.parentNode.children.length < 2) {
                toast('This is the last one — there has to be at least one', 'warn');
                return;
            }

            row.parentNode.removeChild(row);
            document.dispatchEvent(new CustomEvent('planora:rowsChanged'));
        });
    }

    /* Give a cloned subtree fresh ids and repoint every reference to them, so
       duplicated rows keep working labels and ARIA wiring. */
    var cloneSeq = 0;

    // Other modules clone rows too (preset questions), so the re-keying is
    // reachable through an event rather than only from initRepeaters.
    document.addEventListener('planora:rekey', function (event) {
        if (event.detail) rekey(event.detail);
    });

    function rekey(scope) {
        cloneSeq += 1;

        all('[id]', scope).forEach(function (node) {
            var oldId = node.id;
            var newId = oldId + '-' + cloneSeq;
            node.id = newId;

            all('[for="' + oldId + '"]', scope).forEach(function (label) {
                label.setAttribute('for', newId);
            });

            ['aria-controls', 'aria-labelledby', 'aria-describedby', 'data-reveal',
                'data-upload', 'data-upload-dropzone'].forEach(function (attr) {
                    all('[' + attr + '="' + oldId + '"]', scope).forEach(function (ref) {
                        ref.setAttribute(attr, newId);
                    });
                });
        });
    }

    /* ------------------------------------------------------------------------
       9. Conditional disclosure
       --------------------------------------------------------------------------
       Any control carrying data-reveal="<id>" shows or hides that element.
       Works for checkboxes (change) and for switch buttons (click), so an
       opt-in and a toggle behave identically.
       ------------------------------------------------------------------------ */

    function initReveals() {
        // A control may only ever hide something (data-reveal-inverse with no
        // data-reveal), so both attributes have to be collected here.
        all('[data-reveal], [data-reveal-inverse]').forEach(function (control) {
            // A control may reveal several blocks and hide others at the same time
            // (data-reveal-inverse) — e.g. multi-venue vs single-venue.
            var shown = ids(control.getAttribute('data-reveal'));
            var hidden = ids(control.getAttribute('data-reveal-inverse'));

            if (!shown.length && !hidden.length) return;

            function sync(isOn) {
                shown.forEach(function (el) { el.hidden = !isOn; });
                hidden.forEach(function (el) { el.hidden = isOn; });
                control.setAttribute('aria-expanded', String(isOn));

                // The schedule's venue column only makes sense with several venues.
                if (control.id === 'multivenue') {
                    all('[data-schedule] .slots').forEach(function (group) {
                        group.classList.toggle('slots--single', !isOn);
                    });
                    document.body.classList.toggle('is-single-venue', !isOn);
                }

                document.dispatchEvent(new CustomEvent('planora:rowsChanged'));
            }

            if (control.type === 'checkbox') {
                control.addEventListener('change', function () { sync(control.checked); });
                sync(control.checked);
                return;
            }

            // Radios: any sibling in the group can switch the block off again, so the
            // whole group is watched rather than this one input.
            if (control.type === 'radio' && control.name) {
                all('input[type="radio"][name="' + control.name + '"]').forEach(function (peer) {
                    peer.addEventListener('change', function () { sync(control.checked); });
                });
                sync(control.checked);
                return;
            }

            control.addEventListener('click', function () {
                // Switch buttons flip their own aria-checked in initSwitches; read it
                // once that has happened.
                window.setTimeout(function () {
                    sync(control.getAttribute('aria-checked') === 'true');
                }, 0);
            });

            sync(control.getAttribute('aria-checked') === 'true');
        });
    }

    function ids(list) {
        if (!list) return [];

        return list.split(',').map(function (id) {
            return document.getElementById(id.trim());
        }).filter(Boolean);
    }

    /* ------------------------------------------------------------------------
       10. Choice cards — event type
       ------------------------------------------------------------------------
       Superseded by the inline script on EventType.cshtml, which saves the
       choice via a real form POST to Session (see SessionExtensions.cs)
       instead of rewriting hrefs with ?type=. Kept as a no-op registration
       point in case another page adds a [data-choice-group] later.
       ------------------------------------------------------------------------ */

    function initChoiceCards() {
        all('[data-choice-group]').forEach(function (group) {
            var cards = all('[data-choice]', group);
            var field = document.getElementById(group.getAttribute('data-choice-field') || '');

            cards.forEach(function (card) {
                card.addEventListener('click', function () {
                    var type = card.getAttribute('data-choice');

                    cards.forEach(function (other) {
                        var isThis = other === card;
                        other.classList.toggle('choice--selected', isThis);
                        other.setAttribute('aria-checked', String(isThis));
                    });

                    if (field) field.value = type;
                });
            });
        });
    }

    /* ------------------------------------------------------------------------
       11. Segmented controls
       ------------------------------------------------------------------------ */

    function initSegments() {
        all('[data-segment]').forEach(function (group) {
            var buttons = all('[data-segment-value]', group);

            buttons.forEach(function (button) {
                button.addEventListener('click', function () {
                    buttons.forEach(function (other) {
                        other.setAttribute('aria-pressed', String(other === button));
                    });

                    all('[data-segment-panel]').forEach(function (panel) {
                        if (panel.getAttribute('data-segment-owner') !== group.id) return;
                        panel.hidden = panel.getAttribute('data-segment-panel')
                            !== button.getAttribute('data-segment-value');
                    });
                });
            });
        });
    }

    /* ------------------------------------------------------------------------
       12. Seating mode — scoped to a single ticket tier
       ------------------------------------------------------------------------ */

    function initSeatModes() {
        all('[data-seatmode-group]').forEach(function (group) {
            var buttons = all('[data-seatmode]', group);
            var scope = group.closest('[data-seatplan]') || document;
            var panels = all('[data-seatmode-panel]', scope);

            buttons.forEach(function (button) {
                button.addEventListener('click', function () {
                    var mode = button.getAttribute('data-seatmode');

                    buttons.forEach(function (other) {
                        other.setAttribute('aria-pressed', String(other === button));
                    });

                    panels.forEach(function (panel) {
                        panel.hidden = panel.getAttribute('data-seatmode-panel') !== mode;
                    });
                });
            });
        });
    }

    /* ------------------------------------------------------------------------
       13. Accordions
       ------------------------------------------------------------------------ */

    function initAccordions() {
        all('[data-accordion]').forEach(function (head) {
            var panel = document.getElementById(head.getAttribute('aria-controls'));
            if (!panel) return;

            head.addEventListener('click', function () {
                var isOpen = head.getAttribute('aria-expanded') === 'true';
                head.setAttribute('aria-expanded', String(!isOpen));
                panel.hidden = isOpen;
            });
        });
    }

    /* ------------------------------------------------------------------------
       14. Pressed-option groups (publish timing, charge model, ...)
       ------------------------------------------------------------------------ */

    function initPickGroups() {
        all('[data-pick-group]').forEach(function (group) {
            var options = all('[data-pick]', group);

            options.forEach(function (option) {
                option.addEventListener('click', function () {
                    options.forEach(function (other) {
                        // Some of these groups are ARIA tabs rather than toggle buttons, so
                        // whichever state attribute the markup uses is the one kept true.
                        other.setAttribute('aria-pressed', String(other === option));
                        if (other.hasAttribute('aria-selected')) {
                            other.setAttribute('aria-selected', String(other === option));
                        }
                    });

                    all('[data-pick-panel]').forEach(function (panel) {
                        if (panel.getAttribute('data-pick-owner') !== group.id) return;
                        panel.hidden = panel.getAttribute('data-pick-panel')
                            !== option.getAttribute('data-pick');
                    });
                });
            });
        });
    }

    /* ------------------------------------------------------------------------
       15. CSV member-list upload
       --------------------------------------------------------------------------
       Swaps the drop area for the parsed member directory. Real parsing happens
       server-side; here the chosen file name and row count are surfaced so the
       host can confirm the right list was attached.
       ------------------------------------------------------------------------ */

    function initUploads() {
        all('[data-upload]').forEach(function (input) {
            var directory = document.getElementById(input.getAttribute('data-upload'));
            if (!directory) return;

            var nameEl = directory.querySelector('[data-upload-name]');
            var dropzone = document.getElementById(input.getAttribute('data-upload-dropzone') || '');

            input.addEventListener('change', function () {
                if (!input.files || !input.files.length) return;

                if (nameEl) nameEl.textContent = input.files[0].name;
                directory.hidden = false;
                if (dropzone) dropzone.hidden = true;
            });
        });

        all('[data-upload-remove]').forEach(function (button) {
            button.addEventListener('click', function () {
                var directory = button.closest('[data-upload-directory]');
                if (!directory) return;

                var dropzone = document.getElementById(directory.getAttribute('data-upload-dropzone') || '');
                var named = directory.querySelector('[data-upload-name]');

                directory.hidden = true;
                if (dropzone) dropzone.hidden = false;

                toast('<b>' + (named ? named.textContent.trim() : 'The file')
                    + '</b> removed — upload another to replace it', 'warn');
            });
        });
    }

    /* ------------------------------------------------------------------------
       16. Modal dialogs
       --------------------------------------------------------------------------
       [data-modal-open="<id>"] opens, [data-modal-close] and the scrim close.
       Focus moves to the first field on open and returns to the trigger on
       close; Escape closes. Body scroll is locked while a dialog is up.
       ------------------------------------------------------------------------ */

    function initModals() {
        // Close handling is bound for every dialog on the page, whether or not it
        // has a [data-modal-open] trigger — some dialogs are opened by other
        // modules (seat picker, form preview) and must still be dismissible.
        var openers = all('[data-modal-open]');
        var lastTrigger = null;

        function close(modal) {
            modal.hidden = true;
            modal.classList.remove('modal--stacked');

            // Only release the scroll lock once nothing is left open.
            var stillOpen = all('[data-modal]').some(function (other) {
                return other !== modal && !other.hidden;
            });
            if (!stillOpen) document.body.classList.remove('is-locked');

            if (lastTrigger) lastTrigger.focus();
        }

        function open(modal, trigger) {
            lastTrigger = trigger;

            // A dialog opened from inside another dialog has to paint above it, and
            // DOM order alone will not guarantee that.
            var alreadyOpen = all('[data-modal]').some(function (other) {
                return other !== modal && !other.hidden;
            });
            modal.classList.toggle('modal--stacked', alreadyOpen);

            modal.hidden = false;
            document.body.classList.add('is-locked');

            // Prefer the first real field so keyboard users land where they type;
            // dialogs without fields fall back to the first button.
            var first = modal.querySelector(
                'input:not([type="hidden"]):not([type="radio"]), select, textarea'
            ) || modal.querySelector('input, button');

            if (first) first.focus();
        }

        openers.forEach(function (opener) {
            var modal = document.getElementById(opener.getAttribute('data-modal-open'));
            if (!modal) return;

            opener.addEventListener('click', function () { open(modal, opener); });
        });

        all('[data-modal]').forEach(function (modal) {
            all('[data-modal-close]', modal).forEach(function (button) {
                button.addEventListener('click', function () { close(modal); });
            });

            // Clicking the scrim (but not the dialog) dismisses.
            modal.addEventListener('click', function (event) {
                if (event.target === modal) close(modal);
            });
        });

        document.addEventListener('keydown', function (event) {
            if (event.key !== 'Escape') return;

            // Escape dismisses the dialog on top, not the whole stack.
            var open = all('[data-modal]').filter(function (modal) { return !modal.hidden; });
            var stacked = open.filter(function (modal) {
                return modal.classList.contains('modal--stacked');
            });

            var target = stacked.length ? stacked[stacked.length - 1] : open[open.length - 1];
            if (target) close(target);
        });
    }

    /* ------------------------------------------------------------------------
       17. Status filtering
       --------------------------------------------------------------------------
       Buttons in [data-filter-group] carry data-filter="<status>"; every
       [data-filter-item] carries data-status. "all" shows everything. Works
       across both the table rows and the card grid at once, updates the result
       count, and shows an empty state when a filter matches nothing.
       ------------------------------------------------------------------------ */

    function initFilters() {
        all('[data-filter-group]').forEach(function (group) {
            var buttons = all('[data-filter]', group);
            var items = all('[data-filter-item]');
            var counters = all('[data-filter-count]');
            var empties = all('[data-filter-empty]');
            var labels = all('[data-filter-empty-label]');

            function apply(status) {
                var matched = {};

                items.forEach(function (item) {
                    var match = status === 'all' || item.getAttribute('data-status') === status;
                    item.hidden = !match;
                    // Each event appears twice (table row + card) under one data-key,
                    // so count distinct keys rather than elements.
                    if (match) matched[item.getAttribute('data-key')] = true;
                });

                var shown = Object.keys(matched).length;

                counters.forEach(function (counter) { counter.textContent = shown; });
                empties.forEach(function (empty) { empty.hidden = shown > 0; });
                labels.forEach(function (label) {
                    label.textContent = status === 'all' ? 'events' : status + ' events';
                });
            }

            buttons.forEach(function (button) {
                button.addEventListener('click', function () {
                    buttons.forEach(function (other) {
                        var attr = other.hasAttribute('aria-selected') ? 'aria-selected' : 'aria-pressed';
                        other.setAttribute(attr, String(other === button));
                    });

                    apply(button.getAttribute('data-filter'));
                });
            });
        });
    }

    /* ------------------------------------------------------------------------
       18. Settings section navigation
       ------------------------------------------------------------------------ */

    function initSettingsNav() {
        var nav = document.querySelector('[data-settings-nav]');
        if (!nav) return;

        var links = all('[data-section]', nav);
        var panels = all('[data-section-panel]');

        links.forEach(function (link) {
            link.addEventListener('click', function (event) {
                event.preventDefault();
                var section = link.getAttribute('data-section');

                links.forEach(function (other) {
                    other.setAttribute('aria-current', other === link ? 'true' : 'false');
                });

                panels.forEach(function (panel) {
                    panel.hidden = panel.getAttribute('data-section-panel') !== section;
                });

                if (window.history && window.history.replaceState) {
                    window.history.replaceState(null, '', '#' + section);
                }
            });
        });

        // Deep links: settings.html#security opens that section, and browser
        // back/forward between sections keeps working.
        function openFromHash() {
            var hash = window.location.hash.replace('#', '');
            if (!hash) return;

            var target = nav.querySelector('[data-section="' + hash + '"]');
            if (target) target.click();
        }

        window.addEventListener('hashchange', openFromHash);
        openFromHash();
    }

    /* ------------------------------------------------------------------------
       19. Two-stage publish dialog
       --------------------------------------------------------------------------
       Confirm step swaps to a progress step, then hands off to the events list.
       ------------------------------------------------------------------------ */

    function initPublish() {
        var trigger = document.querySelector('[data-publish-confirm]');
        if (!trigger) return;

        var confirmStep = document.querySelector('[data-publish-step="confirm"]');
        var workingStep = document.querySelector('[data-publish-step="working"]');
        var destination = trigger.getAttribute('data-publish-to');

        trigger.addEventListener('click', function () {
            if (confirmStep) confirmStep.hidden = true;
            if (workingStep) workingStep.hidden = false;

            if (!destination) return;

            window.setTimeout(function () {
                window.location.assign(destination);
            }, 2600);
        });
    }

    /* ------------------------------------------------------------------------
       20. City -> timezone, applied automatically
       --------------------------------------------------------------------------
       Hosts should never pick a timezone by hand: the city they are running the
       event in determines it. The select carries the zone on each option so the
       mapping lives with the data, not in this file.
       ------------------------------------------------------------------------ */

    function initTimezone() {
        all('[data-city]').forEach(function (select) {
            var output = document.getElementById(select.getAttribute('data-city'));
            if (!output) return;

            function sync() {
                var option = select.options[select.selectedIndex];
                var zone = option ? option.getAttribute('data-zone') : '';

                output.value = zone || '';
            }

            select.addEventListener('change', sync);
            sync();
        });
    }

    /* ------------------------------------------------------------------------
       21. Inline row edit — agenda builder
       --------------------------------------------------------------------------
       Rows read as text until you press edit; the inputs already hold the
       values, so committing simply copies them back into the display cells.
       ------------------------------------------------------------------------ */

    function initInlineEdit() {
        function cells(row) {
            return all('[data-edit-field]', row);
        }

        function toEdit(row) {
            row.classList.add('is-editing');
            var first = row.querySelector('[data-edit-field] input, [data-edit-field] select');
            if (first) first.focus();
        }

        function commit(row) {
            cells(row).forEach(function (cell) {
                var control = cell.querySelector('input, select');
                var display = cell.querySelector('[data-edit-display]');
                if (!control || !display) return;

                var value = control.tagName === 'SELECT'
                    ? control.options[control.selectedIndex].textContent.trim()
                    : control.value;

                display.textContent = value || '&mdash;';
            });

            row.classList.remove('is-editing');
        }

        document.addEventListener('click', function (event) {
            if (!event.target.closest) return;

            var edit = event.target.closest('[data-edit-toggle]');
            if (edit) {
                toEdit(edit.closest('[data-editrow]'));
                return;
            }

            var save = event.target.closest('[data-edit-save]');
            if (save) commit(save.closest('[data-editrow]'));
        });

        // A freshly added row starts in edit mode — it has nothing to show yet.
        document.addEventListener('planora:rowsChanged', function () {
            all('[data-editrow]').forEach(function (row) {
                var display = row.querySelector('[data-edit-display]');
                if (display && !display.textContent.trim()) row.classList.add('is-editing');
            });
        });
    }

    /* ------------------------------------------------------------------------
       22 + 23. Schedule validation — hall conflicts and capacity
       --------------------------------------------------------------------------
       Three checks a real venue contract depends on:
  
         a) the same hall must not host overlapping slots on one day
         b) a slot may not seat more people than its hall physically holds
         c) peak *concurrent* allocation must fit the venue
  
       Note (b) and (c) are deliberately not a naive sum of every slot: parallel
       breakouts and sequential days both inflate that number, so capacity is
       measured as the busiest simultaneous moment instead.
       ------------------------------------------------------------------------ */

    function initSchedule() {
        all('[data-schedule]').forEach(function (schedule) {
            var conflictNote = schedule.querySelector('[data-schedule-conflict]');
            var conflictText = schedule.querySelector('[data-schedule-conflict-text]');
            var overNote = schedule.querySelector('[data-cap-warning]');
            var overText = schedule.querySelector('[data-cap-warning-text]');
            var limitEl = schedule.querySelector('[data-cap-limit]');
            var peakEl = schedule.querySelector('[data-cap-total]');
            var freeEl = schedule.querySelector('[data-cap-remaining]');

            function minutes(value) {
                if (!value) return null;
                var parts = value.split(':');
                return (parseInt(parts[0], 10) * 60) + parseInt(parts[1], 10);
            }

            function number(value) {
                var n = parseInt(value, 10);
                return isNaN(n) ? 0 : n;
            }

            /* Hall name -> seats, read from whichever venue block is visible. */
            function hallCapacities() {
                var map = {};

                all('.hall').forEach(function (row) {
                    if (row.closest('[hidden]')) return;

                    var name = row.querySelector('.hall__name');
                    var cap = row.querySelector('.hall__cap');
                    if (!name || !cap || !name.value) return;

                    map[name.value] = number(cap.value);
                });

                return map;
            }

            function readSlots() {
                var days = [];

                all('[data-day]', schedule).forEach(function (day) {
                    var dateInput = day.querySelector('[data-day-date]');
                    var slots = [];

                    all('[data-slot]', day).forEach(function (slot) {
                        var hall = slot.querySelector('[data-slot-hall]');
                        var start = minutes(slot.querySelector('[data-slot-start]').value);
                        var end = minutes(slot.querySelector('[data-slot-end]').value);
                        var seats = number(slot.querySelector('[data-cap-input]').value);

                        slot.classList.remove('is-conflict');
                        if (start === null || end === null || !hall) return;

                        slots.push({
                            el: slot, hall: hall.value, start: start, end: end, seats: seats
                        });
                    });

                    days.push({ date: dateInput ? dateInput.value : '', slots: slots });
                });

                return days;
            }

            /* Busiest simultaneous moment: sweep each slot start and add up every
               slot open at that instant. */
            function peakConcurrency(slots) {
                var peak = 0;

                slots.forEach(function (probe) {
                    var atThisMoment = 0;

                    slots.forEach(function (other) {
                        if (other.start <= probe.start && other.end > probe.start) {
                            atThisMoment += other.seats;
                        }
                    });

                    if (atThisMoment > peak) peak = atThisMoment;
                });

                return peak;
            }

            function check() {
                var halls = hallCapacities();
                var days = readSlots();
                var clashes = [];
                var oversized = [];
                var peak = 0;

                days.forEach(function (day) {
                    // (a) overlapping slots in one hall
                    day.slots.forEach(function (a, i) {
                        day.slots.slice(i + 1).forEach(function (b) {
                            if (a.hall !== b.hall) return;
                            if (a.start >= b.end || b.start >= a.end) return;

                            a.el.classList.add('is-conflict');
                            b.el.classList.add('is-conflict');
                            if (clashes.indexOf(a.hall) === -1) clashes.push(a.hall);
                        });
                    });

                    // (b) slot bigger than the hall it sits in
                    day.slots.forEach(function (slot) {
                        var seats = halls[slot.hall];
                        if (seats === undefined || slot.seats <= seats) return;

                        slot.el.classList.add('is-conflict');
                        if (oversized.indexOf(slot.hall) === -1) oversized.push(slot.hall);
                    });

                    // (c) peak concurrent allocation for the day
                    var dayPeak = peakConcurrency(day.slots);
                    if (dayPeak > peak) peak = dayPeak;
                });

                var ceiling = 0;
                Object.keys(halls).forEach(function (name) { ceiling += halls[name]; });

                if (limitEl) limitEl.textContent = ceiling.toLocaleString();
                if (peakEl) peakEl.textContent = peak.toLocaleString();
                if (freeEl) freeEl.textContent = Math.max(0, ceiling - peak).toLocaleString();

                if (conflictNote) {
                    conflictNote.hidden = clashes.length === 0;
                    if (conflictText && clashes.length) {
                        conflictText.textContent = clashes.join(', ')
                            + (clashes.length > 1 ? ' have' : ' has')
                            + ' overlapping time slots on the same day.';
                    }
                }

                if (overNote) {
                    var over = oversized.length > 0 || peak > ceiling;
                    overNote.hidden = !over;

                    if (overText && over) {
                        overText.textContent = oversized.length
                            ? oversized.join(', ') + ' is allocated more seats than the hall holds.'
                            : 'Peak concurrent allocation exceeds the total hall capacity.';
                    }
                }
            }

            schedule.addEventListener('input', check);
            schedule.addEventListener('change', check);
            document.addEventListener('planora:rowsChanged', check);
            check();
        });
    }

    /* ------------------------------------------------------------------------
       24. Event-type feature gating
       --------------------------------------------------------------------------
       Step 1 decides whether the event is public or private, and that answer
       changes which fields the rest of the wizard should even ask for: a
       private event is never listed in discovery, has no payout step, etc.

       The type is now rendered server-side into <body data-event-type="...">
       by _CreateEventLayout.cshtml, sourced from Session (see
       Extensions/SessionExtensions.cs) — never the URL, since a redirect
       drops query strings and silently reverted this to "public" partway
       through the wizard.

         [data-type-only="private"]   shown only for that type
         [data-type-hide="virtual"]   hidden for that type
         [data-type-label]            prints the current type
         [data-type-lock="public"]    control is disabled for that type
       ------------------------------------------------------------------------ */

    var TYPE_NAMES = { public: 'Public Event', private: 'Private Event' };

    function currentType() {
        var attr = document.body.getAttribute('data-event-type');
        if (attr === 'public' || attr === 'private') return attr;

        // Fallback only for pages rendered outside the wizard layout.
        var match = /[?&]type=(public|private)/.exec(window.location.search);
        return match ? match[1] : 'public';
    }

    function initEventType() {
        var type = currentType();
        document.body.setAttribute('data-event-type', type);

        function listed(value) {
            return value ? value.split(/\s+/) : [];
        }

        all('[data-type-only]').forEach(function (el) {
            el.hidden = listed(el.getAttribute('data-type-only')).indexOf(type) === -1;
        });

        all('[data-type-hide]').forEach(function (el) {
            el.hidden = listed(el.getAttribute('data-type-hide')).indexOf(type) !== -1;
        });

        all('[data-type-lock]').forEach(function (el) {
            var locked = listed(el.getAttribute('data-type-lock')).indexOf(type) !== -1;
            el.disabled = locked;
            if (locked) el.setAttribute('aria-disabled', 'true');
            else el.removeAttribute('aria-disabled');
        });

        all('[data-type-label]').forEach(function (el) {
            el.textContent = TYPE_NAMES[type];
        });

        // Auto-selected radios: every group whose default this type mandates.
        all('[data-type-default="' + type + '"]').forEach(function (input) {
            input.checked = true;
        });
    }

    /* ------------------------------------------------------------------------
       25. Wizard step loader (shimmer)
       --------------------------------------------------------------------------
       Full-page loading state for the Create Event wizard: shown the moment a
       host clicks any [data-wizard-link] or submits a wizard form, so every
       step gets a loading indicator with no per-page wiring. Uses a shimmer
       skeleton (see .wizard-loading / .shimmer in styles.css) rather than a
       spinner.
       ------------------------------------------------------------------------ */

    /* ------------------------------------------------------------------------
       25. Wizard step loader (shimmer)
       ------------------------------------------------------------------------ */

    function initWizardLoader() {
        var overlay = document.getElementById('wizard-loading');
        if (!overlay) return;

        function show() {
            overlay.classList.add('is-active');
            overlay.setAttribute('aria-hidden', 'false');
        }

        // Wizard nav links (sidebar steps, Back/Cancel, footer links).
        all('[data-wizard-link]').forEach(function (link) {
            link.addEventListener('click', function (event) {
                var href = link.getAttribute('href');

                if (link.getAttribute('aria-disabled') === 'true' || link.classList.contains('wizard__step--disabled')) {
                    event.preventDefault();
                    return;
                }

                if (!href || href === '' || href.indexOf('#') === 0) return;
                if (event.defaultPrevented || event.metaKey || event.ctrlKey) return;

                show();
            });
        });

        // Wizard step forms (Continue/Save buttons that POST).
        all('.wizard__body form, .wizard__foot form').forEach(function (form) {
            form.addEventListener('submit', function (event) {
                // Check form validity before showing loading state
                if (!form.checkValidity()) {
                    event.preventDefault();
                    return;
                }

                if (event.defaultPrevented) return;
                show();
            });
        });

        // Buttons that submit a form via the "form" attribute
        all('button[type="submit"][form]').forEach(function (button) {
            button.addEventListener('click', function (event) {
                if (button.disabled) return;

                var formId = button.getAttribute('form');
                var form = document.getElementById(formId);

                if (form) {
                    // If the form has missing required fields, block submission and trigger validation
                    if (!form.checkValidity()) {
                        event.preventDefault();
                        form.reportValidity();
                        return;
                    }
                }

                if (event.defaultPrevented) return;
                show();
            });
        });

        // Restore the page instantly if navigating back from cache (bfcache)
        window.addEventListener('pageshow', function (event) {
            if (event.persisted) {
                overlay.classList.remove('is-active');
                overlay.setAttribute('aria-hidden', 'true');
            }
        });
    }

    /* ------------------------------------------------------------------------
       25. Numeric steppers
       --------------------------------------------------------------------------
       Small +/- control for values a host nudges rather than types, like the
       extra-serving limit on a menu item. Clamped to the input's own min/max.
       ------------------------------------------------------------------------ */

    function initSteppers() {
        document.addEventListener('click', function (event) {
            if (!event.target.closest) return;

            var button = event.target.closest('[data-step]');
            if (!button) return;

            var stepper = button.closest('[data-stepper]');
            if (!stepper) return;

            var input = stepper.querySelector('input');
            if (!input) return;

            var min = input.hasAttribute('min') ? parseInt(input.min, 10) : 0;
            var max = input.hasAttribute('max') ? parseInt(input.max, 10) : 99;
            var value = parseInt(input.value, 10);
            if (isNaN(value)) value = min;

            value += parseInt(button.getAttribute('data-step'), 10);
            input.value = Math.min(max, Math.max(min, value));

            input.dispatchEvent(new Event('input', { bubbles: true }));
        });
    }

    /* ------------------------------------------------------------------------
       26. Hub date/slot scoping
       --------------------------------------------------------------------------
       A multi-day event has separate guests, menus and insights per day and per
       slot. Day tabs pick the day; the slot chips below re-render for that day;
       everything carrying data-scope is filtered to the selection.
       ------------------------------------------------------------------------ */

    function initScopes() {
        all('[data-scope-group]').forEach(function (group) {
            var dayTabs = all('[data-day-scope]', group);
            var slotRows = all('[data-slot-scope-for]', group);
            var label = document.querySelector('[data-scope-label]');

            function applySlot(day, slot) {
                all('[data-scope]').forEach(function (item) {
                    var scope = item.getAttribute('data-scope');
                    item.hidden = !(scope === 'all' || scope === day || scope === day + ':' + slot);
                });

                if (!label) return;

                var dayTab = group.querySelector('[data-day-scope="' + day + '"]');
                // Slot keys repeat across days, so look the chip up inside this day's row.
                var slotRow = group.querySelector('[data-slot-scope-for="' + day + '"]');
                var slotChip = slotRow
                    ? slotRow.querySelector('[data-slot-scope="' + slot + '"]')
                    : null;

                label.textContent = (dayTab ? dayTab.getAttribute('data-scope-name') : day)
                    + (slotChip ? ' · ' + slotChip.getAttribute('data-scope-name') : '');
            }

            function selectDay(day) {
                dayTabs.forEach(function (tab) {
                    tab.setAttribute('aria-selected', String(tab.getAttribute('data-day-scope') === day));
                });

                var activeSlots = null;

                slotRows.forEach(function (row) {
                    var mine = row.getAttribute('data-slot-scope-for') === day;
                    row.hidden = !mine;
                    if (mine) activeSlots = row;
                });

                if (!activeSlots) return applySlot(day, 'all');

                var chips = all('[data-slot-scope]', activeSlots);
                chips.forEach(function (chip, index) {
                    chip.setAttribute('aria-pressed', String(index === 0));
                });

                chips.forEach(function (chip) {
                    chip.onclick = function () {
                        chips.forEach(function (other) {
                            other.setAttribute('aria-pressed', String(other === chip));
                        });
                        applySlot(day, chip.getAttribute('data-slot-scope'));
                    };
                });

                applySlot(day, chips.length ? chips[0].getAttribute('data-slot-scope') : 'all');
            }

            dayTabs.forEach(function (tab) {
                tab.addEventListener('click', function () {
                    selectDay(tab.getAttribute('data-day-scope'));
                });
            });

            if (dayTabs.length) selectDay(dayTabs[0].getAttribute('data-day-scope'));
        });
    }

    /* ------------------------------------------------------------------------
       27. Seat picker
       --------------------------------------------------------------------------
       Every seat is a button. Selecting one opens the seat dialog populated from
       the seat's own data, so the host can release, reserve or check a guest in.
       ------------------------------------------------------------------------ */

    var SEAT_STATES = {
        available: 'Available', checked: 'Checked in', vip: 'VIP — sold',
        taken: 'Sold — not arrived', reserved: 'Reserved'
    };

    function initSeatPicker() {
        var seats = all('[data-seat]');
        if (!seats.length) return;

        var dialog = document.getElementById('seat-modal');
        var nameEl = document.querySelector('[data-seatinfo-name]');
        var stateEl = document.querySelector('[data-seatinfo-state]');
        var guestEl = document.querySelector('[data-seatinfo-guest]');

        seats.forEach(function (seat) {
            seat.addEventListener('click', function () {
                seats.forEach(function (other) {
                    other.setAttribute('aria-pressed', String(other === seat));
                });

                var state = seat.getAttribute('data-seat-state') || 'available';

                if (nameEl) nameEl.textContent = seat.getAttribute('data-seat');
                if (stateEl) stateEl.textContent = SEAT_STATES[state] || state;
                if (guestEl) {
                    guestEl.textContent = seat.getAttribute('data-seat-guest') || 'Unassigned';
                }

                if (dialog) {
                    dialog.hidden = false;
                    document.body.classList.add('is-locked');
                }
            });
        });
    }

    /* ------------------------------------------------------------------------
       28. Availability toggles — menu items in the hub
       ------------------------------------------------------------------------ */

    function initAvailability() {
        all('[data-availability]').forEach(function (button) {
            // The hub uses menu rows; Food & Orders uses table rows. Either is fine.
            var row = button.closest('[data-menu-row], [data-food-row]');
            if (!row) return;

            button.addEventListener('click', function () {
                var isOut = row.classList.toggle('is-out');

                button.setAttribute('aria-pressed', String(!isOut));
                button.textContent = isOut ? 'Out of stock' : 'Available';
                button.classList.toggle('badge--out', isOut);
                button.classList.toggle('badge--live', !isOut);
            });
        });

        // Inline rename of a menu item
        all('[data-menu-edit]').forEach(function (button) {
            var row = button.closest('[data-menu-row]');
            if (!row) return;

            button.addEventListener('click', function () {
                var editing = row.classList.toggle('is-editing');
                var input = row.querySelector('input[type="text"]');
                var display = row.querySelector('[data-menu-name]');

                if (editing) {
                    if (input) input.focus();
                    return;
                }

                if (input && display && input.value.trim()) display.textContent = input.value.trim();
            });
        });
    }

    /* ------------------------------------------------------------------------
       29. Registration-form preview
       --------------------------------------------------------------------------
       Builds the attendee-facing form from the current field rows, so the host
       sees exactly what they have configured rather than a canned example.
       ------------------------------------------------------------------------ */

    var PREVIEW_HINTS = {
        'Short Text': 'Single line of text', 'Email': 'name@company.com',
        'Phone': '+234 800 000 0000', 'Long Text': 'Several lines of text',
        'Dropdown': 'Select an option', 'Checkbox': 'Yes / No',
        'Date': 'DD / MM / YYYY', 'Number': '0', 'File Upload': 'Choose a file'
    };

    function initFormPreview() {
        // A page can carry more than one builder (registration form, RSVP form), so
        // every trigger names its own dialog and its own list of rows.
        all('[data-form-preview]').forEach(function (trigger) {
            var modal = document.getElementById(trigger.getAttribute('data-form-preview'));
            var target = modal ? modal.querySelector('[data-form-preview-body]') : null;
            var counter = modal ? modal.querySelector('[data-form-preview-count]') : null;
            var listId = trigger.getAttribute('data-form-preview-list') || 'rf-list';
            if (!modal || !target) return;

            trigger.addEventListener('click', function () {
                var rows = all('[data-repeat-row]', document.getElementById(listId));
                target.textContent = '';

                rows.forEach(function (row) {
                    var label = row.querySelector('input[type="text"]');
                    var type = row.querySelector('select');
                    var required = row.querySelector('input[type="checkbox"]');
                    if (!label) return;

                    var kind = type ? type.value : 'Short Text';

                    var field = document.createElement('div');
                    field.className = 'formpreview__field';

                    var caption = document.createElement('span');
                    caption.className = 'formpreview__label';
                    caption.textContent = (label.value || 'Untitled field')
                        + (required && required.checked ? ' *' : '');

                    var mock = document.createElement('span');
                    mock.className = 'formpreview__mock'
                        + (kind === 'Long Text' ? ' formpreview__mock--area' : '');
                    mock.textContent = PREVIEW_HINTS[kind] || kind;

                    field.appendChild(caption);
                    field.appendChild(mock);
                    target.appendChild(field);
                });

                if (counter) counter.textContent = rows.length + ' fields';

                modal.hidden = false;
                document.body.classList.add('is-locked');
            });
        });
    }

    /* ------------------------------------------------------------------------
       30. Automatic session / slot names
       --------------------------------------------------------------------------
       Naming a day or slot is optional: leave it blank and it becomes
       "Session 2", "Slot 3" and so on, numbered by position.
       ------------------------------------------------------------------------ */

    function initAutoNames() {
        function renumber() {
            var seen = {};

            all('[data-autoname]').forEach(function (input) {
                var prefix = input.getAttribute('data-autoname');
                seen[prefix] = (seen[prefix] || 0) + 1;

                var auto = prefix + ' ' + seen[prefix];
                input.placeholder = auto;
                input.setAttribute('data-autoname-value', auto);
            });
        }

        document.addEventListener('blur', function (event) {
            var input = event.target;
            if (!input.hasAttribute || !input.hasAttribute('data-autoname')) return;
            if (input.value.trim()) return;

            input.value = input.getAttribute('data-autoname-value') || '';
        }, true);

        document.addEventListener('planora:rowsChanged', renumber);
        renumber();
    }

    /* ------------------------------------------------------------------------
       31. Cover image preview
       --------------------------------------------------------------------------
       Hosts need to see what they uploaded before publishing, so each selected
       file is rendered as a 16:9 thumbnail with replace and remove actions. The
       first image is flagged as the primary cover.
       ------------------------------------------------------------------------ */

    function initImagePreview() {
        all('[data-image-input]').forEach(function (input) {
            var gallery = document.getElementById(input.getAttribute('data-image-input'));
            if (!gallery) return;

            var counter = document.querySelector('[data-image-count]');
            var dropzone = document.getElementById(input.getAttribute('data-image-dropzone') || '');
            var max = parseInt(input.getAttribute('data-image-max'), 10) || 5;
            var files = [];

            // Keeps the real <input type="file"> in sync with the `files` array,
            // so whatever the gallery shows is exactly what gets submitted.
            function syncInput() {
                var dt = new DataTransfer();
                files.forEach(function (file) { dt.items.add(file); });
                input.files = dt.files;
            }

            function render() {
                gallery.textContent = '';

                files.forEach(function (file, index) {
                    var card = document.createElement('figure');
                    card.className = 'cover' + (index === 0 ? ' cover--primary' : '');

                    var img = document.createElement('img');
                    img.className = 'cover__frame';
                    img.alt = 'Cover image ' + (index + 1) + ': ' + file.name;
                    img.src = window.URL.createObjectURL(file);
                    card.appendChild(img);

                    if (index === 0) {
                        var tag = document.createElement('span');
                        tag.className = 'cover__tag';
                        tag.textContent = 'Primary';
                        card.appendChild(tag);
                    }

                    var bar = document.createElement('figcaption');
                    bar.className = 'cover__bar';

                    var name = document.createElement('span');
                    name.className = 'cover__name';
                    name.textContent = file.name;
                    bar.appendChild(name);

                    if (index > 0) {
                        bar.appendChild(action('Make primary', 'cover__act', function () {
                            files.unshift(files.splice(index, 1)[0]);
                            render();
                        }, 'M4 12.5 9 17.5 20 6.5'));
                    }

                    bar.appendChild(action('Replace', 'cover__act', function () {
                        input.click();
                    }, 'M4 4v6h6M20 20v-6h-6M20 9A8 8 0 0 0 6 5M4 15a8 8 0 0 0 14 4'));

                    bar.appendChild(action('Remove', 'cover__act cover__act--danger', function () {
                        files.splice(index, 1);
                        render();
                    }, 'M6 6l12 12M18 6L6 18'));

                    card.appendChild(bar);
                    gallery.appendChild(card);
                });

                if (counter) counter.textContent = files.length + '/' + max;
                if (dropzone) dropzone.hidden = files.length >= max;
                gallery.hidden = files.length === 0;

                syncInput();
            }

            function action(label, className, onClick, path) {
                var button = document.createElement('button');
                button.type = 'button';
                button.className = className;
                button.innerHTML = '<span class="u-visually-hidden">' + label + '</span>'
                    + '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" '
                    + 'stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">'
                    + '<path d="' + path + '"/></svg>';
                button.addEventListener('click', onClick);
                return button;
            }

            input.addEventListener('change', function () {
                Array.prototype.forEach.call(input.files || [], function (file) {
                    if (files.length < max) files.push(file);
                });

                render();
            });

            render();
        });
    }

    /* ------------------------------------------------------------------------
       32. Bullet-aware textareas
       --------------------------------------------------------------------------
       Terms and "things to know" read as lists, so Enter starts a new bullet and
       Enter on an empty bullet ends the list.
       ------------------------------------------------------------------------ */

    var BULLET = '\u2022 ';

    function initBullets() {
        all('[data-bullets]').forEach(function (area) {
            area.addEventListener('keydown', function (event) {
                if (event.key !== 'Enter' || event.shiftKey) return;

                var value = area.value;
                var caret = area.selectionStart;
                var lineStart = value.lastIndexOf('\n', caret - 1) + 1;
                var line = value.slice(lineStart, caret);

                event.preventDefault();

                // Enter on an empty bullet closes the list instead of adding another.
                if (line.trim() === BULLET.trim()) {
                    area.value = value.slice(0, lineStart) + '\n' + value.slice(caret);
                    area.selectionStart = area.selectionEnd = lineStart + 1;
                    return;
                }

                var insert = '\n' + BULLET;
                area.value = value.slice(0, caret) + insert + value.slice(caret);
                area.selectionStart = area.selectionEnd = caret + insert.length;
            });

            // First keystroke in an empty field opens the list.
            area.addEventListener('focus', function () {
                if (area.value.trim() === '') area.value = BULLET;
            });

            area.addEventListener('blur', function () {
                if (area.value.trim() === BULLET.trim()) area.value = '';
            });
        });
    }

    /* ------------------------------------------------------------------------
       33. Dynamic wizard step numbering
       --------------------------------------------------------------------------
       Some steps only exist for certain event types (RSVP is private-only), so
       the "Step X of N" label and the progress bar are derived from the steps
       actually visible in the rail rather than hard-coded.
       ------------------------------------------------------------------------ */

    function initStepCount() {
        var indexEl = document.querySelector('[data-step-index]');
        var totalEl = document.querySelector('[data-step-total]');
        if (!indexEl || !totalEl) return;

        var items = all('.wizard__steps > li').filter(function (li) {
            return !li.hidden && li.offsetParent !== null;
        });

        if (!items.length) return;

        var position = 0;
        items.forEach(function (li, i) {
            if (li.querySelector('[aria-current="step"]')) position = i + 1;
        });

        if (!position) return;

        indexEl.textContent = position;
        totalEl.textContent = items.length;

        var pct = Math.round((position - 1) / (items.length - 1) * 100);
        var value = document.querySelector('.wizard__progress-value');
        var fill = document.querySelector('.wizard__progress .meter__fill');

        if (value) value.textContent = pct + '%';
        if (fill) fill.setAttribute('data-fill', String(pct));
    }

    /* ------------------------------------------------------------------------
       34. Session filtering inside the ticket-access picker
       --------------------------------------------------------------------------
       A long schedule is hard to pick from, so date, venue, hall, track and a
       free-text box narrow the session list in place. Filters are per ticket, so
       each picker only touches its own rows.
       ------------------------------------------------------------------------ */

    function initSessionFilter() {
        all('[data-session-scope]').forEach(function (bar) {
            var scope = document.getElementById(bar.getAttribute('data-session-scope'));
            if (!scope) return;

            var controls = all('[data-session-filter]', bar);
            var rows = all('[data-session]', scope);
            var empty = scope.querySelector('[data-session-empty]');

            function apply() {
                var wanted = {};
                controls.forEach(function (control) {
                    wanted[control.getAttribute('data-session-filter')] =
                        control.value.trim().toLowerCase();
                });

                var shown = 0;

                rows.forEach(function (row) {
                    var keep = Object.keys(wanted).every(function (key) {
                        var want = wanted[key];
                        if (!want) return true;

                        var have = (row.getAttribute('data-session-' + key) || '').toLowerCase();
                        return key === 'text' ? have.indexOf(want) !== -1 : have === want;
                    });

                    row.hidden = !keep;
                    if (keep) shown += 1;
                });

                if (empty) empty.hidden = shown !== 0;
            }

            controls.forEach(function (control) {
                control.addEventListener('input', apply);
                control.addEventListener('change', apply);
            });

            apply();
        });
    }

    /* ------------------------------------------------------------------------
       35. "This ticket includes" summary
       --------------------------------------------------------------------------
       Reads the access choices back to the host in the same words they picked,
       so a tier can be checked at a glance: "All standard sessions · Day 1 ·
       VIP Lounge". Rebuilt on every change inside the tier.
       ------------------------------------------------------------------------ */

    function initIncludes() {
        all('[data-includes]').forEach(function (line) {
            var tid = line.getAttribute('data-includes');
            var tier = line.closest('.tier') || line.closest('.freetier')
                || line.parentNode.parentNode;
            var out = line.querySelector('[data-includes-value]');
            if (!tier || !out) return;

            function ticked(selector) {
                return all(selector, tier).filter(function (box) { return box.checked; });
            }

            function label(box) {
                var host = box.closest('label');
                var name = host && host.querySelector('.scopeslot__name, .sessionpick__name, .sendcard__title');
                return name ? name.textContent.replace(/\s+/g, ' ').trim() : '';
            }

            function render() {
                var mode = tier.querySelector('input[name="acc-mode-' + tid + '"]:checked');
                var parts = [];

                if (mode) {
                    if (mode.value === 'all') {
                        parts.push('All standard sessions');
                    } else if (mode.value === 'days') {
                        // "Day 1 — Opening Day" reads better here as just "Day 1", and the
                        // days belong to one phrase: "All standard sessions on Day 1, Day 2".
                        var days = ticked('#accdays-' + tid + ' input[type="checkbox"]')
                            .map(function (box) { return label(box).split('\u2014')[0].trim(); });

                        if (days.length) parts.push('All standard sessions on ' + days.join(', '));
                    } else if (mode.value === 'sessions') {
                        ticked('#accsessions-' + tid + ' input[type="checkbox"]').forEach(function (box) {
                            parts.push(label(box));
                        });
                    }
                }

                all('.sendcard input[type="checkbox"]', tier).forEach(function (box) {
                    if (box.checked) parts.push(label(box));
                });

                out.innerHTML = parts.length ? parts.join(' &middot; ') : 'Nothing selected yet';
            }

            tier.addEventListener('change', render);
            render();
        });
    }

    /* ------------------------------------------------------------------------
       36. Button feedback, and a floor under every button
       --------------------------------------------------------------------------
       Two jobs. [data-action="…"] confirms what just happened in a toast, which
       is how a static build honestly represents work that would hit a server.
       The delegated fallback then catches any button that carries no behaviour at
       all, so a click is never silently swallowed — the accessible name becomes
       the message. Dead buttons are the single most common complaint about a
       prototype, and this removes the whole class of them.
       ------------------------------------------------------------------------ */

    var HOOKS = [
        'data-modal-open', 'data-modal-close', 'data-repeat-add', 'data-repeat-remove',
        'data-view', 'data-switch', 'data-segment-value', 'data-seatmode', 'data-pick',
        'data-accordion', 'data-edit-toggle', 'data-edit-save', 'data-scope-value',
        'data-availability', 'data-form-preview', 'data-step', 'data-filter',
        'data-settings-nav', 'data-publish-confirm', 'data-seat', 'data-sidebar-open',
        'data-sidebar-collapse', 'data-password-toggle', 'data-upload-remove',
        'data-image-act', 'data-action', 'data-tier-hide', 'data-preset',
        'data-broadcast-template', 'data-editor-tool', 'data-color', 'data-role-view'
    ];

    var toastHost = null;

    function toast(message, tone) {
        if (!toastHost) {
            toastHost = document.createElement('div');
            toastHost.className = 'toasts';
            toastHost.setAttribute('role', 'status');
            toastHost.setAttribute('aria-live', 'polite');
            document.body.appendChild(toastHost);
        }

        var note = document.createElement('p');
        note.className = 'toast' + (tone ? ' toast--' + tone : '');
        note.innerHTML = message;
        toastHost.appendChild(note);

        window.setTimeout(function () {
            note.classList.add('toast--out');
            window.setTimeout(function () {
                if (note.parentNode) note.parentNode.removeChild(note);
            }, 300);
        }, 3200);
    }

    function initActions() {
        document.addEventListener('click', function (event) {
            var button = event.target.closest ? event.target.closest('button') : null;
            if (!button || button.disabled) return;

            var message = button.getAttribute('data-action');

            if (!message) {
                // Nothing else claims this button — say what it would have done.
                if (button.type === 'submit' || button.closest('a')) return;
                if (HOOKS.some(function (h) { return button.hasAttribute(h); })) return;

                var label = (button.textContent || '').replace(/\s+/g, ' ').trim()
                    || button.getAttribute('aria-label') || '';
                if (!label) return;

                toast(label);
                return;
            }

            toast(message, button.getAttribute('data-action-tone'));
        });
    }

    /* ------------------------------------------------------------------------
       37. Ticket visibility toggle
       --------------------------------------------------------------------------
       The eye on a tier hides it from buyers without deleting it — the tier keeps
       its price, seating and access rules, but never appears at checkout. Hosts
       use it to stage a tier before sales open, or retire one mid-sale without
       losing what it sold. Hidden tiers are dimmed, badged, and excluded from the
       "revenue if sold out" total.
       ------------------------------------------------------------------------ */

    function initTierVisibility() {
        all('[data-tier-hide]').forEach(function (button) {
            var tier = button.closest('.tier');
            if (!tier) return;

            var name = tier.querySelector('.tier__name');
            var label = button.querySelector('.u-visually-hidden');

            function sync(hidden, announce) {
                tier.classList.toggle('tier--hidden', hidden);
                button.setAttribute('aria-pressed', String(hidden));

                var tierName = name && name.value ? name.value : 'this tier';

                if (label) {
                    label.textContent = (hidden ? 'Show ' : 'Hide ')
                        + tierName + (hidden ? ' to buyers' : ' from buyers');
                }

                if (announce) {
                    toast(hidden
                        ? '<b>' + tierName + '</b> is hidden from buyers &mdash; it keeps its settings'
                        : '<b>' + tierName + '</b> is visible to buyers again');
                }
            }

            button.addEventListener('click', function () {
                sync(button.getAttribute('aria-pressed') !== 'true', true);
            });

            sync(button.getAttribute('aria-pressed') === 'true', false);
        });
    }

    /* ------------------------------------------------------------------------
       38. RSVP tab lock
       --------------------------------------------------------------------------
       An event created with "no RSVP" has nothing to collect, so the hub's RSVP
       tab is locked rather than empty. The prototype reads the answer from the
       query string (?rsvp=off); a real build reads it from the event.
       ------------------------------------------------------------------------ */

    function initRsvpGate() {
        var tab = document.querySelector('[data-rsvp-tab]');
        if (!tab) return;

        var off = /[?&]rsvp=off/.test(window.location.search);
        var locked = document.querySelector('[data-rsvp-locked]');
        var live = document.querySelector('[data-rsvp-live]');

        if (locked) locked.hidden = !off;
        if (live) live.hidden = off;

        tab.disabled = off;
        tab.setAttribute('aria-disabled', String(off));
        tab.classList.toggle('subtabs__tab--locked', off);

        if (off) tab.title = 'RSVP is switched off for this event';
        else tab.removeAttribute('title');
    }

    /* ------------------------------------------------------------------------
       39. One-click preset questions
       --------------------------------------------------------------------------
       Clones the last row of a field builder and renames it, so a common RSVP
       question is one click rather than typing.
       ------------------------------------------------------------------------ */

    function initPresets() {
        all('[data-preset]').forEach(function (button) {
            button.addEventListener('click', function () {
                var list = document.getElementById(button.getAttribute('data-preset'));
                var text = button.getAttribute('data-preset-label');
                if (!list || !text) return;

                var rows = all('[data-repeat-row]', list);
                var source = rows[rows.length - 1];
                if (!source) return;

                var clone = source.cloneNode(true);
                list.appendChild(clone);

                // Cloned ids would collide; initRepeaters exposes the same re-keying.
                document.dispatchEvent(new CustomEvent('planora:rekey', { detail: clone }));

                clone.classList.remove('fieldrow--locked');
                all('input, select', clone).forEach(function (input) {
                    input.disabled = false;
                    input.readOnly = false;
                    if (input.type === 'checkbox') input.checked = false;
                });

                var label = clone.querySelector('input[type="text"]');
                if (label) label.value = text;

                toast('Added &ldquo;' + text + '&rdquo; to the form');
            });
        });
    }

    /* ------------------------------------------------------------------------
       40. Broadcast templates
       --------------------------------------------------------------------------
       A template button opens the composer with the message already written, so a
       live announcement takes one tap and one send.
       ------------------------------------------------------------------------ */

    function initBroadcastTemplates() {
        all('[data-broadcast-template]').forEach(function (button) {
            button.addEventListener('click', function () {
                var body = document.querySelector('[data-broadcast-body]');
                if (!body) return;

                body.value = button.getAttribute('data-broadcast-template');
            });
        });
    }

    /* ------------------------------------------------------------------------
       41. Description editor tools
       --------------------------------------------------------------------------
       The toolbar wraps or prefixes the selection with plain markdown, which the
       landing page renders. Real enough to be useful, small enough to stay
       dependency-free.
       ------------------------------------------------------------------------ */

    var EDITOR_WRAP = { bold: '**', italic: '_' };
    var EDITOR_PREFIX = { h2: '## ', list: '- ', quote: '> ' };

    function initEditorTools() {
        all('[data-editor-tool]').forEach(function (button) {
            button.addEventListener('click', function () {
                var area = document.getElementById(button.getAttribute('data-editor-target'));
                if (!area) return;

                var tool = button.getAttribute('data-editor-tool');
                var start = area.selectionStart;
                var end = area.selectionEnd;
                var picked = area.value.slice(start, end);

                if (EDITOR_WRAP[tool]) {
                    var mark = EDITOR_WRAP[tool];
                    var text = picked || (tool === 'bold' ? 'bold text' : 'italic text');

                    area.value = area.value.slice(0, start) + mark + text + mark
                        + area.value.slice(end);
                    area.selectionStart = start + mark.length;
                    area.selectionEnd = start + mark.length + text.length;
                } else if (tool === 'link') {
                    var shown = picked || 'link text';
                    var link = '[' + shown + '](https://)';

                    area.value = area.value.slice(0, start) + link + area.value.slice(end);
                    area.selectionStart = area.selectionEnd = start + link.length - 1;
                } else if (EDITOR_PREFIX[tool]) {
                    var lineStart = area.value.lastIndexOf('\n', start - 1) + 1;
                    var prefix = EDITOR_PREFIX[tool];

                    area.value = area.value.slice(0, lineStart) + prefix
                        + area.value.slice(lineStart);
                    area.selectionStart = area.selectionEnd = start + prefix.length;
                }

                area.focus();
            });
        });
    }

    /* ------------------------------------------------------------------------
       42. Brand colour presets
       ------------------------------------------------------------------------ */

    function initColorPresets() {
        all('[data-color]').forEach(function (button) {
            var group = all('[data-color-target="'
                + button.getAttribute('data-color-target') + '"]');

            button.addEventListener('click', function () {
                var value = button.getAttribute('data-color');
                var target = button.getAttribute('data-color-target');
                var swatch = document.getElementById(target + '-swatch');
                var text = document.getElementById(target + '-colour');

                if (swatch) {
                    swatch.value = value.toLowerCase();
                    // Anything listening for a colour change (the page preview) has to
                    // hear this the same way it hears a manual pick.
                    swatch.dispatchEvent(new Event('input', { bubbles: true }));
                }
                if (text) text.value = value.toUpperCase();

                group.forEach(function (other) {
                    other.setAttribute('aria-pressed', String(other === button));
                });
            });
        });
    }

    /* ------------------------------------------------------------------------
       43. Carrying the RSVP answer through to Event Hub
       --------------------------------------------------------------------------
       The wizard's yes/no decides whether the hub's RSVP tab is usable. In the
       prototype that travels in the query string, the same way the event type
       does, so the locked state can actually be seen.
       ------------------------------------------------------------------------ */

    function initRsvpFlag() {
        var radios = all('[data-rsvp-flag]');
        if (!radios.length) return;

        function sync() {
            var picked = radios.filter(function (radio) { return radio.checked; })[0];
            var flag = picked ? picked.getAttribute('data-rsvp-flag') : 'on';

            all('[data-rsvp-link]').forEach(function (link) {
                link.setAttribute('href',
                    link.getAttribute('href').replace(/rsvp=(on|off)/, 'rsvp=' + flag));
            });
        }

        radios.forEach(function (radio) {
            radio.addEventListener('change', sync);
        });

        sync();
    }

    /* ------------------------------------------------------------------------
       44. Role-based access
       --------------------------------------------------------------------------
       One organisation, several users, four predefined roles, one role per user.
       The whole mechanism is two attributes:
  
         [data-role-only="admin manager"]  visible only to those roles
         [data-role-hide="admin"]          hidden from those roles
  
       Sidebar items, page-level buttons and single table cells all use the same
       pair, so gating something new is one attribute rather than a code change.
       The active role rides in the query string (?role=finance) here; a real build
       reads it from the signed-in user.
       ------------------------------------------------------------------------ */

    var ROLE_LABELS = {
        admin: 'Admin', manager: 'Event Manager',
        finance: 'Finance', viewer: 'Viewer'
    };

    /* A page a role may not open is refused in words rather than left blank: a
       blank screen reads as a broken app, and the host still needs somewhere to
       go. The gate sits on the page's <main>, stamped at build time from one
       permission table, so a new page is gated by listing it rather than by
       remembering to add markup. */
    function guardPage(role) {
        var main = document.querySelector('[data-page-roles]');
        if (!main) return;

        var allowed = main.getAttribute('data-page-roles').split(/\s+/).indexOf(role) !== -1;
        var block = document.querySelector('[data-role-block]');

        if (allowed) {
            main.hidden = false;
            if (block) block.hidden = true;
            return;
        }

        main.hidden = true;

        if (!block) {
            block = document.createElement('div');
            block.setAttribute('data-role-block', '');
            block.className = 'roleblock';
            main.parentNode.insertBefore(block, main);
        }

        block.hidden = false;
        block.innerHTML =
            '<section class="lockcard"><span class="tile tile--lg tile--muted"'
            + ' aria-hidden="true"></span><div class="lockcard__body">'
            + '<h2 class="lockcard__title">Not available to your role</h2>'
            + '<p class="lockcard__text">You are signed in as <b>' + ROLE_LABELS[role]
            + '</b>, and this screen is not part of that role. Nothing is missing from '
            + 'the app &mdash; it is simply not yours to open.</p>'
            + '<p class="lockcard__actions">'
            + '<a class="btn btn--primary btn--sm" href="dashboard.html?role=' + role
            + '">Back to Dashboard</a>'
            // Only offered to a role that can actually open it — pointing somebody at a
            // second refusal is worse than offering nothing.
            + (role === 'admin'
                ? '<a class="btn btn--quiet btn--sm" href="team-roles.html">'
                + 'See what each role can reach</a>'
                : '')
            + '</p></div></section>';
    }

    /* Hiding a tab can leave the hidden tab selected, which shows an empty panel
       and looks like a bug. Whenever the role changes, the first tab the role can
       actually see is selected. */
    function reselectTabs() {
        all('[data-viewswitch]').forEach(function (group) {
            var tabs = all('[data-view]', group).filter(function (tab) {
                return !tab.hidden && tab.offsetParent !== null;
            });
            if (!tabs.length) return;

            var live = tabs.filter(function (tab) {
                return tab.getAttribute('aria-selected') === 'true'
                    || tab.getAttribute('aria-pressed') === 'true';
            });

            if (live.length) return;
            tabs[0].click();
        });
    }

    function initRoles() {
        var match = /[?&]role=(admin|manager|finance|viewer)/.exec(window.location.search);
        var role = match ? match[1] : 'admin';

        function apply(next) {
            role = next;
            document.body.setAttribute('data-role', role);

            all('[data-role-only]').forEach(function (el) {
                el.hidden = el.getAttribute('data-role-only').split(/\s+/).indexOf(role) === -1;
            });

            all('[data-role-hide]').forEach(function (el) {
                el.hidden = el.getAttribute('data-role-hide').split(/\s+/).indexOf(role) !== -1;
            });

            all('[data-role-label]').forEach(function (el) {
                el.textContent = ROLE_LABELS[role];
            });

            all('[data-role-view]').forEach(function (button) {
                button.setAttribute('aria-pressed',
                    String(button.getAttribute('data-role-view') === role));
            });

            guardPage(role);
            reselectTabs();

            // Keep the role on every in-app link so it survives navigation.
            all('a[href]').forEach(function (link) {
                var href = link.getAttribute('href');
                if (!href || href.charAt(0) === '#' || /^(https?:|mailto:|tel:)/.test(href)) return;

                if (/[?&]role=/.test(href)) {
                    link.setAttribute('href', href.replace(/role=[a-z]+/, 'role=' + role));
                } else if (role !== 'admin') {
                    var mark = href.indexOf('#');
                    var base = mark === -1 ? href : href.slice(0, mark);
                    var hash = mark === -1 ? '' : href.slice(mark);

                    link.setAttribute('href', base + (base.indexOf('?') === -1 ? '?' : '&')
                        + 'role=' + role + hash);
                }
            });
        }

        // Viewing as a restricted role hides Team & Roles, so there has to be a way
        // back that does not depend on that menu still being there.
        function banner() {
            var bar = document.querySelector('[data-role-banner]');

            if (role === 'admin') {
                if (bar) bar.parentNode.removeChild(bar);
                return;
            }

            if (!bar) {
                bar = document.createElement('p');
                bar.className = 'rolebanner';
                bar.setAttribute('data-role-banner', '');
                document.body.appendChild(bar);
            }

            bar.innerHTML = 'Viewing the app as <b>' + ROLE_LABELS[role]
                + '</b> &mdash; menus and actions this role cannot use are hidden. '
                + '<a href="' + window.location.pathname + '?role=admin">Back to Admin view</a>';
        }

        all('[data-role-view]').forEach(function (button) {
            button.addEventListener('click', function () {
                apply(button.getAttribute('data-role-view'));
                banner();
                toast('Now viewing as <b>' + ROLE_LABELS[role]
                    + '</b> &mdash; the sidebar and gated actions have changed');
            });
        });

        apply(role);
        banner();
    }

    /* ------------------------------------------------------------------------
       45. Pause and resume sales on a menu item
       --------------------------------------------------------------------------
       Pausing takes an item off the guest ordering screen without deleting it or
       losing what it has already sold, so the row's status badge and the button
       itself both have to flip.
       ------------------------------------------------------------------------ */

    function initSalesToggle() {
        all('[data-sales-toggle]').forEach(function (button) {
            var row = button.closest('tr');
            var badge = row ? row.querySelector('[data-sales-status]') : null;
            var label = button.querySelector('.u-visually-hidden');
            var name = button.getAttribute('data-sales-toggle');

            button.addEventListener('click', function () {
                var paused = button.getAttribute('aria-pressed') !== 'true';

                button.setAttribute('aria-pressed', String(paused));
                button.classList.toggle('iconbtn--info', !paused);

                if (badge && badge.textContent.trim() !== 'Out of Stock') {
                    badge.textContent = paused ? 'Paused' : 'Live';
                    badge.className = 'badge badge--nodot badge--'
                        + (paused ? 'paused' : 'live');
                    badge.setAttribute('data-sales-status', '');
                }

                if (label) label.textContent = (paused ? 'Resume sales for ' : 'Pause sales for ') + name;

                toast(paused
                    ? '<b>' + name + '</b> is paused &mdash; guests can no longer order it'
                    : '<b>' + name + '</b> is back on sale');
            });
        });
    }

    /* ------------------------------------------------------------------------
       46. Checking a guest in from the guests table
       --------------------------------------------------------------------------
       The scanner does this at the door, but a host also needs to do it by hand
       for a guest whose QR will not read. The row's check-in badge flips and the
       hero counters move with it.
       ------------------------------------------------------------------------ */

    function initCheckin() {
        all('[data-checkin]').forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.closest('tr');
                var name = button.getAttribute('data-checkin');
                // Find the badge that actually carries the check-in state, wherever the
                // table happens to put it — the hub and the Check-in page differ.
                var badge = all('.badge', row).filter(function (el) {
                    return /^(Checked-in|Not checked-in)$/.test(el.textContent.trim());
                })[0];

                if (badge && badge.textContent.trim() === 'Checked-in') {
                    toast('<b>' + name + '</b> is already checked in');
                    return;
                }

                if (badge) {
                    badge.textContent = 'Checked-in';
                    badge.className = 'badge badge--live';
                }

                // The counters are named rather than found by position, because the hub
                // and the Check-in page order their figures differently.
                var inEl = document.querySelector('[data-count="checkedin"]');
                var leftEl = document.querySelector('[data-count="remaining"]');

                if (inEl && leftEl) {
                    var checked = parseInt(inEl.textContent.replace(/\D/g, ''), 10);
                    var left = parseInt(leftEl.textContent.replace(/\D/g, ''), 10);

                    if (!isNaN(checked) && !isNaN(left) && left > 0) {
                        inEl.textContent = (checked + 1).toLocaleString('en-US');
                        leftEl.textContent = (left - 1).toLocaleString('en-US');
                    }
                }

                // The button has done its job — replace it with the state, as the
                // already-checked-in rows show.
                var done = document.createElement('span');
                done.className = 'badge badge--nodot badge--live';
                done.textContent = 'Checked in';
                if (button.parentNode) button.parentNode.replaceChild(done, button);

                toast('<b>' + name + '</b> checked in');
            });
        });
    }

    /* ------------------------------------------------------------------------
       47. Speaker directory
       --------------------------------------------------------------------------
       A speaker is a record, not a string: photo, title, organisation and bio are
       entered once and reused on every session. This module keeps the directory,
       the session chips and the guest-facing preview in agreement, and shows each
       speaker which sessions they are on.
       ------------------------------------------------------------------------ */

    function speakerRecord(row) {
        function val(name) {
            var el = row.querySelector('[data-speaker-field="' + name + '"]');
            return el ? el.value.trim() : '';
        }

        var name = val('name');

        return {
            key: row.getAttribute('data-speaker-key') || '',
            name: name,
            title: val('title'),
            org: val('org'),
            bio: val('bio'),
            initials: name.split(/\s+/).filter(Boolean).slice(0, 2)
                .map(function (word) { return word.charAt(0).toUpperCase(); }).join(''),
            photo: row.getAttribute('data-speaker-photo-url') || ''
        };
    }

    function speakerDirectory() {
        var out = {};
        all('[data-speaker-row]').forEach(function (row) {
            var rec = speakerRecord(row);
            if (rec.key) out[rec.key] = rec;
        });
        return out;
    }

    function paintAvatar(el, rec) {
        var initials = el.querySelector('.speakerav__initials');
        if (initials) initials.textContent = rec.initials || '?';

        if (rec.photo) {
            el.classList.add('speakerav--photo');
            // A runtime-chosen image can only be applied as a property; there is no
            // authored inline style here and no class could carry an unknown URL.
            el.style.backgroundImage = 'url("' + rec.photo + '")';
        } else {
            el.classList.remove('speakerav--photo');
            el.style.backgroundImage = '';
        }
    }

    function syncSpeakers() {
        var dir = speakerDirectory();

        // Every avatar for a key, wherever it lives.
        Object.keys(dir).forEach(function (key) {
            all('[data-speaker-avatar="' + key + '"]').forEach(function (el) {
                paintAvatar(el, dir[key]);
            });

            all('[data-speaker-name="' + key + '"]').forEach(function (el) {
                el.textContent = dir[key].name;
            });

            var counter = document.querySelector('[data-speaker-count="' + key + '"]');
            if (counter) counter.textContent = String(dir[key].bio.length);
        });

        // Which sessions is each speaker on?
        Object.keys(dir).forEach(function (key) {
            var target = document.querySelector('[data-speaker-sessions="' + key + '"]');
            if (!target) return;

            var sessions = all('[data-speaker-chip="' + key + '"]').map(function (chip) {
                var slot = chip.closest('[data-slot]');
                var nameField = slot ? slot.querySelector('.slotcard__name') : null;
                return nameField && nameField.value ? nameField.value : 'Untitled session';
            });

            target.textContent = sessions.length
                ? 'On ' + sessions.length + ' session' + (sessions.length === 1 ? '' : 's')
                + ': ' + sessions.join(', ')
                : 'Not assigned to a session yet';
        });

        // Keep every session picker offering the current directory.
        all('[data-speaker-add]').forEach(function (picker) {
            var sid = picker.getAttribute('data-speaker-add');
            var chips = document.querySelector('[data-speaker-chips="' + sid + '"]');
            var taken = chips
                ? all('[data-speaker-chip]', chips).map(function (chip) {
                    return chip.getAttribute('data-speaker-chip');
                })
                : [];

            all('option', picker).forEach(function (option) {
                if (!option.value) return;
                option.disabled = taken.indexOf(option.value) !== -1;
            });
        });
    }

    function initSpeakers() {
        if (!document.querySelector('[data-speaker-row]')) return;

        // Photo chosen: preview it immediately on every avatar for that speaker.
        document.addEventListener('change', function (event) {
            var input = event.target.closest
                ? event.target.closest('[data-speaker-photo]') : null;
            if (!input || !input.files || !input.files.length) return;

            var row = input.closest('[data-speaker-row]');
            if (!row) return;

            row.setAttribute('data-speaker-photo-url',
                window.URL.createObjectURL(input.files[0]));
            syncSpeakers();

            var rec = speakerRecord(row);
            toast('Photo added for <b>' + (rec.name || 'this speaker') + '</b>');
        });

        // Typing in the directory updates chips, counts and the preview live.
        document.addEventListener('input', function (event) {
            if (event.target.closest && event.target.closest('[data-speaker-field]')) {
                syncSpeakers();
            }
        });

        // Guest-facing preview.
        all('[data-speaker-preview]').forEach(function (button) {
            button.addEventListener('click', function () {
                var key = button.getAttribute('data-speaker-preview');
                var dir = speakerDirectory();
                var rec = dir[key];
                if (!rec) return;

                var card = document.querySelector('[data-speaker-card]');
                if (!card) return;

                var avatar = card.querySelector('[data-speakercard-avatar]');
                if (avatar) paintAvatar(avatar, rec);

                function put(sel, text) {
                    var el = card.querySelector(sel);
                    if (el) {
                        el.textContent = text;
                        el.hidden = !text;
                    }
                }

                put('[data-speakercard-name]', rec.name || 'Unnamed speaker');
                put('[data-speakercard-role]', [rec.title, rec.org].filter(Boolean).join(' · '));
                put('[data-speakercard-bio]', rec.bio || 'No bio yet.');

                var sessions = document.querySelector('[data-speaker-sessions="' + key + '"]');
                put('[data-speakercard-sessions]', sessions ? sessions.textContent : '');
            });
        });

        document.addEventListener('planora:rowsChanged', syncSpeakers);
        syncSpeakers();
    }

    /* ------------------------------------------------------------------------
       48. Attaching speakers to a session
       --------------------------------------------------------------------------
       The picker adds a chip and disables that option; the chip's own X removes
       it. Each chip carries a hidden input with the speaker key, so the session
       stores a reference rather than a retyped name.
       ------------------------------------------------------------------------ */

    function initSpeakerPick() {
        document.addEventListener('change', function (event) {
            var picker = event.target.closest
                ? event.target.closest('[data-speaker-add]') : null;
            if (!picker || !picker.value) return;

            var sid = picker.getAttribute('data-speaker-add');
            var chips = document.querySelector('[data-speaker-chips="' + sid + '"]');
            var key = picker.value;
            var dir = speakerDirectory();
            var rec = dir[key];

            picker.value = '';
            if (!chips || !rec) return;
            if (chips.querySelector('[data-speaker-chip="' + key + '"]')) return;

            var chip = document.createElement('span');
            chip.className = 'speakerchip';
            chip.setAttribute('data-speaker-chip', key);
            chip.innerHTML =
                '<span class="speakerav speakerav--xs" data-speaker-avatar="' + key + '">'
                + '<span class="speakerav__initials"></span></span>'
                + '<span class="speakerchip__name" data-speaker-name="' + key + '"></span>'
                + '<input type="hidden" name="' + sid + '-speaker[]" value="' + key + '">'
                + '<button class="speakerchip__x" type="button" data-speaker-drop>'
                + '<span class="u-visually-hidden">Remove this speaker from the session</span>'
                + '&times;</button>';

            chips.appendChild(chip);
            syncSpeakers();
            toast('<b>' + rec.name + '</b> added to this session');
        });

        document.addEventListener('click', function (event) {
            var drop = event.target.closest ? event.target.closest('[data-speaker-drop]') : null;
            if (!drop) return;

            var chip = drop.closest('[data-speaker-chip], .speakerchip');
            if (!chip) return;

            chip.parentNode.removeChild(chip);
            syncSpeakers();
        });
    }

    /* ------------------------------------------------------------------------
       49. Reusable form templates
       --------------------------------------------------------------------------
       A saved template is a <template> element holding a list of fields — label,
       answer type, required, locked. Because it is data rather than markup for one
       particular builder, the same library loads into the registration form and
       the RSVP form even though their rows differ: the loader clones the first
       existing row of the target list and rewrites it.
       ------------------------------------------------------------------------ */

    function templateFields(key) {
        var host = document.querySelector('[data-formtemplate="' + key + '"]');
        if (!host) return null;

        return all('[data-tplfield]', host.content).map(function (item) {
            return {
                label: item.getAttribute('data-label') || '',
                type: item.getAttribute('data-type') || 'Short Text',
                required: item.getAttribute('data-required') === 'true',
                locked: item.getAttribute('data-locked') === 'true'
            };
        });
    }

    function readFields(list) {
        return all('[data-repeat-row]', list).map(function (row) {
            var label = row.querySelector('input[type="text"]');
            var type = row.querySelector('select');
            var req = row.querySelector('input[type="checkbox"]');

            return {
                label: label ? label.value : '',
                type: type ? type.value : 'Short Text',
                required: !!(req && req.checked),
                locked: row.classList.contains('fieldrow--locked')
            };
        });
    }

    function writeRow(row, spec) {
        var label = row.querySelector('input[type="text"]');
        var type = row.querySelector('select');
        var req = row.querySelector('input[type="checkbox"]');

        if (label) label.value = spec.label;
        if (type) type.value = spec.type;
        if (req) req.checked = spec.required;

        row.classList.toggle('fieldrow--locked', spec.locked);

        all('input, select, button', row).forEach(function (control) {
            if (control.hasAttribute('data-repeat-remove')) {
                control.disabled = spec.locked;
                return;
            }
            if (control.tagName === 'BUTTON') return;

            control.disabled = spec.locked && control !== label;
            if (control === label) control.readOnly = spec.locked;
        });
    }

    function applyTemplate(listId, fields, name) {
        var list = document.getElementById(listId);
        if (!list || !fields || !fields.length) return;

        var rows = all('[data-repeat-row]', list);
        var pattern = rows[0];
        if (!pattern) return;

        // Keep one row as the shape to clone, then rebuild the list from the template.
        var shape = pattern.cloneNode(true);
        rows.forEach(function (row) { row.parentNode.removeChild(row); });

        fields.forEach(function (spec, i) {
            var row = shape.cloneNode(true);
            list.appendChild(row);

            // Cloned ids must not collide with the row they came from.
            document.dispatchEvent(new CustomEvent('planora:rekey', { detail: row }));

            var no = row.querySelector('.fieldrow__no');
            if (no) no.textContent = String(i + 1);

            writeRow(row, spec);
        });

        document.dispatchEvent(new CustomEvent('planora:rowsChanged'));

        var note = document.querySelector('[data-tplnote="' + listId + '"]');
        if (note) {
            note.innerHTML = 'Loaded <b>' + name + '</b> &mdash; ' + fields.length
                + ' fields. Edit freely; the template itself is unchanged.';
        }

        toast('<b>' + name + '</b> loaded &mdash; ' + fields.length + ' fields');
    }

    function initFormTemplates() {
        var savedFrom = null;

        all('[data-tplload]').forEach(function (button) {
            button.addEventListener('click', function () {
                var listId = button.getAttribute('data-tplload');
                var picker = document.querySelector('[data-tplpick="' + listId + '"]');
                if (!picker || !picker.value) {
                    toast('Choose a template first');
                    return;
                }

                var key = picker.value;
                var name = picker.options[picker.selectedIndex].text.split(' · ')[0];
                applyTemplate(listId, templateFields(key), name);
            });
        });

        // "Use this template" from the library targets whichever builder is on the page.
        all('[data-tpluse]').forEach(function (button) {
            button.addEventListener('click', function () {
                var key = button.getAttribute('data-tpluse');
                var picker = document.querySelector('[data-tplpick]');
                if (!picker) return;

                var listId = picker.getAttribute('data-tplpick');
                picker.value = key;

                var name = document.querySelector('[data-formtemplate="' + key + '"]')
                    .getAttribute('data-formtemplate-name');

                applyTemplate(listId, templateFields(key), name);
            });
        });

        // Saving: snapshot the current rows into a new <template> plus a picker option.
        all('[data-tplsave-source]').forEach(function (button) {
            button.addEventListener('click', function () {
                savedFrom = button.getAttribute('data-tplsave-source');

                var list = document.getElementById(savedFrom);
                if (!list) return;

                var fields = readFields(list);
                var count = document.querySelector('[data-tplsave-count]');
                var names = document.querySelector('[data-tplsave-list]');

                if (count) count.textContent = fields.length + ' fields';
                if (names) {
                    names.textContent = fields.map(function (f) { return f.label; })
                        .filter(Boolean).join(' · ');
                }
            });
        });

        var confirm = document.querySelector('[data-tplsave-confirm]');
        if (confirm) {
            confirm.addEventListener('click', function () {
                var nameField = document.getElementById('tplname');
                var name = nameField && nameField.value.trim();
                if (!savedFrom || !name) {
                    toast('Give the template a name first');
                    return;
                }

                var list = document.getElementById(savedFrom);
                var fields = readFields(list);
                var key = 'saved-' + name.toLowerCase().replace(/[^a-z0-9]+/g, '-');

                var host = document.createElement('template');
                host.setAttribute('data-formtemplate', key);
                host.setAttribute('data-formtemplate-name', name);
                host.innerHTML = '<ul>' + fields.map(function (f) {
                    return '<li data-tplfield data-label="' + f.label + '" data-type="' + f.type
                        + '"' + (f.required ? ' data-required="true"' : '')
                        + (f.locked ? ' data-locked="true"' : '') + '></li>';
                }).join('') + '</ul>';
                document.body.appendChild(host);

                all('[data-tplpick]').forEach(function (picker) {
                    var option = document.createElement('option');
                    option.value = key;
                    option.textContent = name + ' · ' + fields.length + ' fields';
                    picker.appendChild(option);
                });

                if (nameField) nameField.value = '';
                toast('Saved <b>' + name + '</b> &mdash; available to your whole organisation');
            });
        }
    }

    /* ------------------------------------------------------------------------
       50. Pre-publish system check
       --------------------------------------------------------------------------
       The checklist tells a host what they filled in. This tells them what is
       actually wrong: capacity that cannot be honoured, halls double-booked,
       required settings left half-done. Every rule reads the draft snapshot rather
       than being hard-coded to a result, and a blocker disables Publish.
  
       Severity is deliberate. A blocker is something that would break the event or
       mislead a buyer. A warning is something a host may have meant.
       ------------------------------------------------------------------------ */

    function syscheckRules(d) {
        function n(key) { return parseInt(d.getAttribute('data-' + key) || '0', 10); }
        function is(key) { return d.getAttribute('data-' + key) === '1'; }
        function val(key) { return d.getAttribute('data-' + key) || ''; }

        var out = [];
        function add(level, title, detail, step, href) {
            out.push({ level: level, title: title, detail: detail, step: step, href: href });
        }

        var paid = val('pricing') === 'paid';
        var released = n('tickets-released');
        var peak = n('peak-capacity');
        var halls = n('hall-capacity');

        // --- capacity -------------------------------------------------------
        if (released > halls) {
            add('block', 'More tickets than the venue holds',
                released.toLocaleString('en-US') + ' tickets released against '
                + halls.toLocaleString('en-US') + ' seats across all halls. '
                + (released - halls).toLocaleString('en-US')
                + ' buyers could arrive with nowhere to sit.',
                'Tickets & Pricing', 'tickets-pricing.html');
        } else if (released > peak && peak > 0) {
            add('warn', 'Tickets exceed peak concurrent capacity',
                released.toLocaleString('en-US') + ' tickets against a peak of '
                + peak.toLocaleString('en-US')
                + ' concurrent seats. Fine if attendance is staggered across sessions, '
                + 'a problem if everyone shows up for the keynote.',
                'Schedule Builder', 'schedule-builder.html');
        } else {
            add('pass', 'Capacity reconciles', 'Tickets released fit within hall capacity.',
                'Tickets & Pricing', 'tickets-pricing.html');
        }

        // --- schedule integrity ---------------------------------------------
        if (n('hall-conflicts') > 0) {
            add('block', 'Halls are double-booked',
                n('hall-conflicts') + ' hall' + (n('hall-conflicts') === 1 ? ' is' : 's are')
                + ' hosting two overlapping sessions on the same day.',
                'Schedule Builder', 'schedule-builder.html');
        } else {
            add('pass', 'No hall conflicts',
                'Every session has a hall to itself for its whole slot.',
                'Schedule Builder', 'schedule-builder.html');
        }

        if (n('slot-over-hall') > 0) {
            add('block', 'A session is allocated more seats than its hall holds',
                n('slot-over-hall') + ' session' + (n('slot-over-hall') === 1 ? '' : 's')
                + ' exceed the capacity of the hall they are in.',
                'Schedule Builder', 'schedule-builder.html');
        }

        if (n('sessions') === 0) {
            add('warn', 'No sessions in the schedule',
                'The event page will have no agenda. Fine for a single-slot party, odd for a '
                + 'conference.', 'Schedule Builder', 'schedule-builder.html');
        } else if (n('agenda-entries') && n('agenda-entries') < n('sessions')) {
            add('warn', 'Agenda is thinner than the schedule',
                n('agenda-entries') + ' agenda entries for ' + n('sessions')
                + ' sessions — some sessions will publish without a printed running order.',
                'Schedule Builder', 'schedule-builder.html');
        }

        // --- speakers -------------------------------------------------------
        if (n('sessions-no-speaker') > 0) {
            add('warn', 'Sessions with no speaker',
                n('sessions-no-speaker') + ' session'
                + (n('sessions-no-speaker') === 1 ? '' : 's')
                + ' will publish with no one named against them.',
                'Schedule Builder', 'schedule-builder.html');
        }

        if (n('speakers-no-bio') > 0) {
            add('warn', 'Speakers without a bio',
                n('speakers-no-bio') + ' speaker profile'
                + (n('speakers-no-bio') === 1 ? '' : 's') + ' has only a name, so the event page '
                + 'shows nothing about them.', 'Schedule Builder', 'schedule-builder.html');
        }

        if (n('speakers') > 0 && n('speakers-no-photo') === n('speakers')) {
            add('warn', 'No speaker photos',
                'All ' + n('speakers') + ' speakers are showing initials instead of a photo. '
                + 'Speaker photos are the most-clicked part of an event page.',
                'Schedule Builder', 'schedule-builder.html');
        }

        // --- identity and page ----------------------------------------------
        if (n('cover-images') === 0) {
            if (val('discovery') === 'listed') {
                add('block', 'No cover image on a listed event',
                    'A listed event with no cover image shows a grey placeholder in discovery, '
                    + 'in search results and on every share.',
                    'Basic Details', 'basic-details.html');
            } else {
                add('warn', 'No cover image',
                    'The page and shared links will use a placeholder.',
                    'Basic Details', 'basic-details.html');
            }
        } else {
            add('pass', 'Cover image set',
                n('cover-images') + ' image' + (n('cover-images') === 1 ? '' : 's')
                + ' uploaded, first one is the hero.', 'Basic Details', 'basic-details.html');
        }

        if (!is('terms')) {
            add('warn', 'No terms and conditions',
                'Buyers will accept nothing at checkout, which leaves you without a record.',
                'Content Sections', 'content-sections.html');
        }

        // --- commerce -------------------------------------------------------
        if (paid && !is('payout-complete')) {
            add('block', 'Payout account incomplete',
                'This event sells tickets but has no complete bank account, so takings cannot '
                + 'be settled.', 'Tickets & Pricing', 'tickets-pricing.html');
        } else if (paid) {
            add('pass', 'Payout account set',
                'Bank name, account number and holder name are all present.',
                'Tickets & Pricing', 'tickets-pricing.html');
        }

        if (paid && n('tiers') === 0) {
            add('block', 'Paid event with no ticket tiers',
                'Nothing is on sale, so nobody can register.',
                'Tickets & Pricing', 'tickets-pricing.html');
        }

        if (n('tiers-no-access') > 0) {
            add('block', 'A ticket grants nothing',
                n('tiers-no-access') + ' tier has no session access and no extras — a buyer '
                + 'would pay for nothing.', 'Tickets & Pricing', 'tickets-pricing.html');
        }

        if (n('tiers-no-sale-end') > 0) {
            add('warn', 'Tiers with no sale end date',
                n('tiers-no-sale-end') + ' tier' + (n('tiers-no-sale-end') === 1 ? '' : 's')
                + ' will stay on sale until the event starts. Set an end date if you need a '
                + 'catering headcount first.', 'Tickets & Pricing', 'tickets-pricing.html');
        }

        if (n('seating-set') > 0 && n('seating-set') < n('tiers')) {
            add('warn', 'Seating set on some tiers only',
                n('seating-set') + ' of ' + n('tiers') + ' tiers assign seats. The rest are '
                + 'unseated, which is legitimate but worth confirming.',
                'Tickets & Pricing', 'tickets-pricing.html');
        }

        var exposure = n('discount-exposure');
        var revenue = n('potential-revenue');
        if (exposure && revenue && exposure / revenue > 0.25) {
            add('warn', 'Discount exposure is high',
                'If every promo code is fully redeemed you give away '
                + Math.round(exposure / revenue * 100) + '% of potential revenue.',
                'Add-ons & Coupons', 'addons-coupons.html');
        }

        // --- registration and RSVP ------------------------------------------
        if (n('regform-fields') === 0) {
            add('block', 'Registration form is empty',
                'Not even a name is being collected.', 'Access Control', 'access-control.html');
        } else {
            add('pass', 'Registration form ready',
                n('regform-fields') + ' fields, name and email included.',
                'Access Control', 'access-control.html');
        }

        if (is('rsvp-required') && n('rsvp-questions') === 0) {
            add('block', 'RSVP is on but has no questions',
                'Guests would be asked to reply to a form with nothing in it.',
                'RSVP Setup', 'rsvp-setup.html');
        }

        if (is('passcode-required') && !is('passcode-set')) {
            add('block', 'Passcode required but not set',
                'The page would lock everyone out, including invited guests.',
                'Branding & Page', 'branding-page.html');
        }

        // --- food -----------------------------------------------------------
        if (is('food-enabled') && !is('food-window-covers-sessions')) {
            add('warn', 'Serving window does not cover every session',
                'Guests in sessions outside the window will find ordering closed.',
                'Food & Beverage', 'food-beverage.html');
        }

        return out;
    }

    var SYSCHECK_LEVEL = {
        block: { label: 'Blocker', cls: 'block' },
        warn: { label: 'Warning', cls: 'warn' },
        pass: { label: 'Passed', cls: 'pass' }
    };

    function initSysCheck() {
        var snapshot = document.querySelector('[data-syscheck]');
        var runner = document.querySelector('[data-syscheck-run]');
        if (!snapshot || !runner) return;

        var out = document.querySelector('[data-syscheck-out]');
        var idle = document.querySelector('[data-syscheck-idle]');
        var list = document.querySelector('[data-syscheck-list]');
        var gate = document.querySelector('[data-publish-gate]');
        var blockNote = document.querySelector('[data-publish-block]');

        function render() {
            var findings = syscheckRules(snapshot);
            var counts = { block: 0, warn: 0, pass: 0 };

            findings.forEach(function (f) { counts[f.level] += 1; });

            list.textContent = '';

            ['block', 'warn', 'pass'].forEach(function (level) {
                findings.filter(function (f) { return f.level === level; })
                    .forEach(function (f) {
                        var row = document.createElement('div');
                        row.className = 'finding finding--' + SYSCHECK_LEVEL[level].cls;
                        row.innerHTML =
                            '<span class="finding__level">' + SYSCHECK_LEVEL[level].label + '</span>'
                            + '<span class="finding__body">'
                            + '<span class="finding__title">' + f.title + '</span>'
                            + '<span class="finding__detail">' + f.detail + '</span>'
                            + '</span>'
                            + (level === 'pass' ? ''
                                : '<a class="finding__fix" href="' + f.href + '" data-wizard-link>Fix in '
                                + f.step + '</a>');
                        list.appendChild(row);
                    });
            });

            document.querySelector('[data-syscheck-blockers]').textContent = counts.block;
            document.querySelector('[data-syscheck-warnings]').textContent = counts.warn;
            document.querySelector('[data-syscheck-passes]').textContent = counts.pass;

            var clear = document.querySelector('[data-syscheck-clear]');
            if (clear) clear.hidden = counts.block > 0;

            if (idle) idle.hidden = true;
            out.hidden = false;

            if (gate) {
                gate.disabled = counts.block > 0;
                gate.setAttribute('aria-disabled', String(counts.block > 0));
            }

            if (blockNote) {
                blockNote.hidden = counts.block === 0;
                var count = blockNote.querySelector('[data-publish-block-count]');
                if (count) count.textContent = counts.block;
            }

            var ran = document.querySelector('[data-syscheck-ran]');
            if (ran) {
                ran.textContent = 'Checked ' + findings.length + ' rules just now';
            }

            toast(counts.block
                ? '<b>' + counts.block + ' blocker' + (counts.block === 1 ? '' : 's')
                + '</b> found — publishing is held until they are fixed'
                : 'System check passed with ' + counts.warn + ' warning'
                + (counts.warn === 1 ? '' : 's'));
        }

        runner.addEventListener('click', render);
    }

    /* ------------------------------------------------------------------------
       51. Broadcast audience builder
       --------------------------------------------------------------------------
       Segments are built from the event's own attributes — tier, VIP status,
       attendance, day, session, hall, RSVP state, food orders, add-ons, seating.
       Options inside a group are OR, groups are AND, and an empty group means
       "any", which is what a host expects.
  
       The match count is computed, not written: guests are grouped into cohorts
       carrying their attributes and headcount, and the filter sums the cohorts
       that satisfy every active group.
       ------------------------------------------------------------------------ */

    function segCohorts() {
        return all('[data-cohort]').map(function (el) {
            return {
                label: el.getAttribute('data-label') || '',
                count: parseInt(el.getAttribute('data-count') || '0', 10),
                tokens: (el.getAttribute('data-tokens') || '').split(/\s+/)
            };
        });
    }

    function segSelection() {
        var groups = {};

        all('[data-segopt]').forEach(function (box) {
            var key = box.getAttribute('data-segopt');
            if (!box.checked) return;
            groups[key] = groups[key] || [];
            groups[key].push(box.value);
        });

        return groups;
    }

    function segMatch(cohorts, groups) {
        var keys = Object.keys(groups);

        return cohorts.filter(function (cohort) {
            return keys.every(function (key) {
                return groups[key].some(function (token) {
                    return cohort.tokens.indexOf(token) !== -1;
                });
            });
        });
    }

    function initSegmentBuilder() {
        var builder = document.querySelector('.segbuilder');
        if (!builder) return;

        var cohorts = segCohorts();
        var total = cohorts.reduce(function (sum, c) { return sum + c.count; }, 0);

        function describe(groups) {
            var keys = Object.keys(groups);
            if (!keys.length) return 'No filters — this reaches everyone registered';

            return keys.map(function (key) {
                var labels = groups[key].map(function (token) {
                    var box = builder.querySelector('[data-segopt="' + key + '"][value="' + token + '"]');
                    var span = box ? box.parentNode.querySelector('span') : null;
                    return span ? span.textContent.trim() : token;
                });

                return labels.join(' or ');
            }).join(' AND ');
        }

        function render() {
            var groups = segSelection();
            var matched = segMatch(cohorts, groups);
            var count = matched.reduce(function (sum, c) { return sum + c.count; }, 0);

            all('[data-segcount]').forEach(function (el) {
                el.textContent = count.toLocaleString('en-US');
            });

            var detail = builder.querySelector('[data-segdetail]');
            if (detail) detail.textContent = describe(groups);

            // Per-group "Any" marker, so an untouched group reads as deliberate.
            all('[data-segany]').forEach(function (flag) {
                var key = flag.getAttribute('data-segany');
                var picked = groups[key] ? groups[key].length : 0;
                flag.textContent = picked ? picked + ' selected' : 'Any';
                flag.classList.toggle('seggroup__any--on', picked > 0);
            });

            // Channel reach: WhatsApp coverage is lower than email in this dataset.
            var wa = Math.round(count * 0.87);
            var waEl = builder.querySelector('[data-segwa]');
            var emailEl = builder.querySelector('[data-segemail]');
            if (waEl) waEl.textContent = wa.toLocaleString('en-US');
            if (emailEl) emailEl.textContent = count.toLocaleString('en-US');

            var cost = document.querySelector('[data-segcost]');
            if (cost) {
                var naira = wa * 35 + Math.round(count * 1.5 / 1000);
                cost.innerHTML = 'WhatsApp ' + wa.toLocaleString('en-US') + ' &middot; Email '
                    + count.toLocaleString('en-US') + ' &middot; about &#8358;'
                    + naira.toLocaleString('en-US') + ' from your wallet';
            }

            var saveCount = document.querySelector('[data-segsave-count]');
            var saveDetail = document.querySelector('[data-segsave-detail]');
            if (saveCount) saveCount.textContent = count.toLocaleString('en-US') + ' guests';
            if (saveDetail) saveDetail.textContent = describe(groups);
        }

        all('[data-segopt]').forEach(function (box) {
            box.addEventListener('change', render);
        });

        var saved = builder.querySelector('[data-segsaved]');
        if (saved) {
            saved.addEventListener('change', function () {
                var tokens = saved.value ? saved.value.split(/\s+/) : [];

                all('[data-segopt]').forEach(function (box) {
                    box.checked = tokens.indexOf(box.value) !== -1;
                });

                render();

                if (saved.value) {
                    toast('Loaded <b>' + saved.options[saved.selectedIndex].text + '</b>');
                }
            });
        }

        var clear = builder.querySelector('[data-segclear]');
        if (clear) {
            clear.addEventListener('click', function () {
                all('[data-segopt]').forEach(function (box) { box.checked = false; });
                if (saved) saved.value = '';
                render();
            });
        }

        var confirm = document.querySelector('[data-segsave-confirm]');
        if (confirm) {
            confirm.addEventListener('click', function () {
                var field = document.getElementById('segname');
                var name = field && field.value.trim();
                if (!name) {
                    toast('Give the segment a name first');
                    return;
                }

                var tokens = all('[data-segopt]').filter(function (box) { return box.checked; })
                    .map(function (box) { return box.value; }).join(' ');

                if (saved) {
                    var option = document.createElement('option');
                    option.value = tokens;
                    option.textContent = name;
                    saved.appendChild(option);
                }

                field.value = '';
                toast('Saved <b>' + name + '</b> — your team can reuse it all event');
            });
        }

        render();
    }

    /* ------------------------------------------------------------------------
       52. Branding drives the event page preview
       --------------------------------------------------------------------------
       The preview is the whole guest page, not a thumbnail, so every branding
       control has somewhere real to land: the logo appears in the header and
       footer, the cover fills the hero, the two colours theme buttons, chips and
       links, and the background mode reskins the page.
  
       Colours arrive as arbitrary hex at runtime, so they are applied as custom
       properties on the preview root — the stylesheet still owns every rule, and
       nothing is authored inline in the markup.
       ------------------------------------------------------------------------ */

    function initBrandPreview() {
        var page = document.querySelector('[data-ep-page]');
        if (!page) return;

        function setToken(name, value) {
            page.style.setProperty(name, value);

            var full = document.querySelector('[data-fullpreview-body] [data-ep-page]');
            if (full) full.style.setProperty(name, value);
        }

        function mirrors(selector) {
            // The full-size dialog holds a clone, so both copies stay in step.
            return all(selector);
        }

        /* ---- colours ---- */
        function syncColour(kind) {
            var source = document.querySelector('input[type="color"][data-brand-colour="' + kind + '"]');
            var text = document.querySelector('input[type="text"][data-brand-colour="' + kind + '"]');
            if (!source) return;

            var value = source.value;
            if (text) text.value = value.toUpperCase();
            setToken('--ep-' + kind, value);
        }

        all('[data-brand-colour]').forEach(function (input) {
            input.addEventListener('input', function () {
                var kind = input.getAttribute('data-brand-colour');

                if (input.type === 'text') {
                    var hex = input.value.trim();
                    if (!/^#[0-9a-fA-F]{6}$/.test(hex)) return;

                    var picker = document.querySelector(
                        'input[type="color"][data-brand-colour="' + kind + '"]');
                    if (picker) picker.value = hex.toLowerCase();
                }

                syncColour(kind);
            });
        });

        /* ---- background mode ---- */
        var bg = document.querySelector('[data-brand-bg]');
        if (bg) {
            all('[data-pick]', bg).forEach(function (button) {
                button.addEventListener('click', function () {
                    var mode = button.getAttribute('data-pick');

                    mirrors('[data-ep-page]').forEach(function (el) {
                        el.classList.remove('ep--light', 'ep--dark', 'ep--brand');
                        el.classList.add('ep--' + mode);
                    });
                });
            });
        }

        /* ---- logo and cover ---- */
        function bindImage(inputSel, boxSel, phSel, nameSel, verbSel, clearSel, label) {
            var input = document.querySelector(inputSel);
            if (!input) return;

            function paint(url, filename) {
                mirrors(boxSel).forEach(function (box) {
                    box.classList.toggle('is-set', !!url);
                    box.style.backgroundImage = url ? 'url("' + url + '")' : '';
                });

                var ph = document.querySelector(phSel);
                if (ph) ph.hidden = !!url;

                var name = document.querySelector(nameSel);
                if (name) name.textContent = filename || ('No ' + label + ' yet');

                var verb = document.querySelector(verbSel);
                if (verb) verb.textContent = url ? 'Replace ' + label : 'Upload ' + label;

                var clear = document.querySelector(clearSel);
                if (clear) clear.hidden = !url;

                return url;
            }

            input.addEventListener('change', function () {
                if (!input.files || !input.files.length) return;

                var file = input.files[0];
                var url = window.URL.createObjectURL(file);
                paint(url, file.name);

                if (label === 'logo') {
                    mirrors('[data-ep-logo]').forEach(function (el) {
                        el.classList.add('ep__logo--image');
                        el.style.backgroundImage = 'url("' + url + '")';
                    });
                } else {
                    mirrors('[data-ep-hero]').forEach(function (el) {
                        el.classList.add('is-set');
                        el.style.backgroundImage = 'url("' + url + '")';
                    });
                    var heroPh = document.querySelector('[data-ep-heroph]');
                    if (heroPh) heroPh.hidden = true;
                }

                toast('<b>' + file.name + '</b> applied to the event page');
            });

            var clear = document.querySelector(clearSel);
            if (clear) {
                clear.addEventListener('click', function () {
                    input.value = '';
                    paint('', '');

                    if (label === 'logo') {
                        mirrors('[data-ep-logo]').forEach(function (el) {
                            el.classList.remove('ep__logo--image');
                            el.style.backgroundImage = '';
                        });
                    } else {
                        mirrors('[data-ep-hero]').forEach(function (el) {
                            el.classList.remove('is-set');
                            el.style.backgroundImage = '';
                        });
                        var heroPh = document.querySelector('[data-ep-heroph]');
                        if (heroPh) heroPh.hidden = false;
                    }

                    toast('Removed the ' + label);
                });
            }
        }

        bindImage('[data-brand-logo]', '[data-brand-logo-box]', '[data-brand-logo-ph]',
            '[data-brand-logo-name]', '[data-brand-logo-verb]', '[data-brand-logo-clear]', 'logo');
        bindImage('[data-brand-cover]', '[data-brand-cover-box]', '[data-brand-cover-ph]',
            '[data-brand-cover-name]', '[data-brand-cover-verb]', '[data-brand-cover-clear]', 'cover');

        /* ---- text that the guest page shows ---- */
        function bindText(inputSel, targetSel, fallback) {
            var input = document.querySelector(inputSel);
            if (!input) return;

            input.addEventListener('input', function () {
                var value = input.value.trim() || fallback;
                mirrors(targetSel).forEach(function (el) { el.textContent = value; });
            });
        }

        bindText('[data-brand-name]', '.ep__logotext', 'Acme Global');
        bindText('[data-brand-slug]', '[data-ep-slug]', 'tech-summit-africa-2026');
        bindText('[data-brand-social]', '[data-ep-social]',
            'Two days of builders, buyers and capital');

        /* ---- passcode badge on the browser chrome ---- */
        var passcode = document.querySelector('[data-brand-passcode]');
        if (passcode) {
            passcode.addEventListener('click', function () {
                window.setTimeout(function () {
                    var on = passcode.getAttribute('aria-checked') === 'true';
                    mirrors('[data-ep-lock]').forEach(function (el) { el.hidden = !on; });
                }, 0);
            });
        }

        /* ---- preview width ---- */
        var widths = document.getElementById('epwidth');
        if (widths) {
            all('[data-pick]', widths).forEach(function (button) {
                button.addEventListener('click', function () {
                    var frame = document.querySelector('[data-ep-frame]');
                    if (frame) {
                        frame.classList.toggle('epframe--mobile',
                            button.getAttribute('data-pick') === 'mobile');
                    }
                });
            });
        }

        /* ---- agenda day tabs inside the preview ---- */
        all('[data-ep-daytab]').forEach(function (tab) {
            tab.addEventListener('click', function () {
                var key = tab.getAttribute('data-ep-daytab');

                all('[data-ep-daytab]').forEach(function (other) {
                    other.setAttribute('aria-pressed',
                        String(other.getAttribute('data-ep-daytab') === key));
                });

                all('[data-ep-day]').forEach(function (day) {
                    day.hidden = day.getAttribute('data-ep-day') !== key;
                });
            });
        });

        /* ---- full-size dialog gets a live clone ---- */
        var fullBody = document.querySelector('[data-fullpreview-body]');
        var opener = document.querySelector('[data-modal-open="fullpreview-modal"]');

        if (fullBody && opener) {
            opener.addEventListener('click', function () {
                var clone = document.querySelector('[data-ep-frame]').cloneNode(true);
                clone.classList.remove('epframe--mobile');

                all('[data-ep-frame]', fullBody).forEach(function (old) {
                    old.parentNode.removeChild(old);
                });

                fullBody.appendChild(clone);
            });
        }

        syncColour('primary');
        syncColour('accent');
    }

    /* ------------------------------------------------------------------------
       53. Status-aware row action menus
       --------------------------------------------------------------------------
       The menu contents are generated per status server-side, so this only has to
       open the right one, close it on Escape or an outside click, flip it upward
       when it would fall off the bottom, and carry the event's name into whichever
       dialog the chosen action opens.
       ------------------------------------------------------------------------ */

    function initRowMenus() {
        // The event name has to reach whichever dialog an action opens, and those
        // triggers also live outside row menus (Settings does the same thing), so
        // this is bound before the early return.
        document.addEventListener('click', function (event) {
            var item = event.target.closest ? event.target.closest('[data-event-name]') : null;
            if (!item) return;

            var name = item.getAttribute('data-event-name');
            all('[data-event-target]').forEach(function (el) { el.textContent = name; });
        });

        var triggers = all('[data-rowmenu]');
        if (!triggers.length) return;

        function closeAll(except) {
            triggers.forEach(function (trigger) {
                var menu = document.getElementById(trigger.getAttribute('data-rowmenu'));
                if (!menu || menu === except) return;

                menu.hidden = true;
                menu.classList.remove('rowmenu--up');
                menu.classList.remove('rowmenu--float');
                trigger.setAttribute('aria-expanded', 'false');
            });
        }

        /* Tables scroll sideways, and a scroll container clips its overflow on both
           axes, so a menu positioned inside one is cut off at the row. The open menu
           is therefore lifted into the viewport layer and placed by hand: right edge
           aligned to the trigger, dropping down unless the space below is short. */
        function place(trigger, menu) {
            menu.classList.add('rowmenu--float');
            menu.style.top = '0px';
            menu.style.left = '0px';

            var anchor = trigger.getBoundingClientRect();
            var box = menu.getBoundingClientRect();
            var gap = 6;
            var margin = 12;

            var left = anchor.right - box.width;
            left = Math.min(left, window.innerWidth - box.width - margin);
            left = Math.max(margin, left);

            var below = window.innerHeight - anchor.bottom - gap - margin;
            var above = anchor.top - gap - margin;
            var top = anchor.bottom + gap;

            if (box.height > below && above > below) top = anchor.top - gap - box.height;
            top = Math.max(margin, Math.min(top, window.innerHeight - box.height - margin));

            menu.style.left = left + 'px';
            menu.style.top = top + 'px';
        }

        triggers.forEach(function (trigger) {
            var menu = document.getElementById(trigger.getAttribute('data-rowmenu'));
            if (!menu) return;

            trigger.addEventListener('click', function (event) {
                event.stopPropagation();

                var opening = menu.hidden;
                closeAll(menu);

                menu.hidden = !opening;
                trigger.setAttribute('aria-expanded', String(opening));

                if (!opening) {
                    menu.classList.remove('rowmenu--float');
                    return;
                }

                place(trigger, menu);

                var first = menu.querySelector('.rowmenu__item');
                if (first) first.focus();
            });
        });

        document.addEventListener('click', function (event) {
            if (event.target.closest && event.target.closest('.rowmenu, [data-rowmenu]')) return;
            closeAll(null);
        });

        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') closeAll(null);
        });

        // A floated menu no longer travels with its row, so scrolling closes it.
        window.addEventListener('scroll', function () { closeAll(null); }, true);
        window.addEventListener('resize', function () { closeAll(null); });
    }

    /* ------------------------------------------------------------------------
       54. Cancellation scope and the refund it triggers
       --------------------------------------------------------------------------
       Cancelling a whole event, a date or a single slot refund different amounts
       to different numbers of guests, so the figures are summed from the scope the
       host actually picked rather than stated once. The submit stays disabled until
       something is selected, a reason is given and the consequence is acknowledged.
       ------------------------------------------------------------------------ */

    function initCancelScope() {
        var modal = document.getElementById('cancelevent-modal');
        if (!modal) return;

        var scopes = all('[data-cancel-scope]', modal);
        var items = all('[data-cancel-item]', modal);
        var reason = modal.querySelector('#cancel-reason');
        var confirm = modal.querySelector('[data-cancel-confirm]');
        var submit = modal.querySelector('[data-cancel-submit]');

        function naira(value) {
            return '₦' + value.toLocaleString('en-US');
        }

        function render() {
            var scope = scopes.filter(function (s) { return s.checked; })[0];
            var mode = scope ? scope.getAttribute('data-cancel-scope') : 'event';

            var refund = 0;
            var guests = 0;
            var picked = 0;

            if (mode === 'event') {
                refund = parseInt(scope.getAttribute('data-cancel-refund'), 10);
                guests = parseInt(scope.getAttribute('data-cancel-guests'), 10);
                picked = 1;
            } else {
                var group = document.getElementById(mode === 'dates' ? 'cancel-dates' : 'cancel-slots');

                all('[data-cancel-item]', group).forEach(function (box) {
                    if (!box.checked) return;
                    picked += 1;
                    refund += parseInt(box.getAttribute('data-cancel-refund'), 10);
                    guests += parseInt(box.getAttribute('data-cancel-guests'), 10);
                });
            }

            var commission = Math.round(refund * 0.02);

            function put(selector, text) {
                var el = modal.querySelector(selector);
                if (el) el.textContent = text;
            }

            put('[data-cancel-guestcount]', guests.toLocaleString('en-US'));
            put('[data-cancel-amount]', naira(refund));
            put('[data-cancel-commission]', naira(commission));
            put('[data-cancel-total]', naira(refund));

            put('[data-cancel-cta]', mode === 'event'
                ? 'Cancel entire event'
                : 'Cancel ' + picked + ' ' + (mode === 'dates' ? 'date' : 'slot')
                + (picked === 1 ? '' : 's'));

            put('[data-cancel-note]', mode === 'event'
                ? 'The event moves to Cancelled. Non-ticketed guests are notified but nothing is refunded.'
                : 'The rest of the event goes ahead. Only guests holding tickets for the selected '
                + (mode === 'dates' ? 'dates' : 'slots') + ' are refunded.');

            if (submit) {
                submit.disabled = !picked
                    || !(reason && reason.value.trim())
                    || !(confirm && confirm.checked);
            }
        }

        scopes.concat(items).forEach(function (input) {
            input.addEventListener('change', render);
        });

        // "Choose dates" / "Choose slots" in Settings open the dialog already on
        // that scope, so the host does not repeat a choice they just made.
        all('[data-cancel-preset]').forEach(function (button) {
            button.addEventListener('click', function () {
                var want = button.getAttribute('data-cancel-preset');
                var pick = modal.querySelector('[data-cancel-scope="' + want + '"]');
                if (!pick) return;

                pick.checked = true;
                pick.dispatchEvent(new Event('change', { bubbles: true }));
            });
        });
        if (reason) reason.addEventListener('input', render);
        if (confirm) confirm.addEventListener('change', render);

        render();
    }

    /* ------------------------------------------------------------------------
       55. Vendors, entry staff and their PINs
       --------------------------------------------------------------------------
       A vendor works a counter on a given afternoon; a scanner works one door for
       one session. So an assignment is (event, dates, slots) and the dialog can
       also cover the whole event in one action. Everyone gets a PIN because that
       is how they sign in on the counter or scanner app — it is generated on save,
       can be regenerated, and can be sent to the number on file.
       ------------------------------------------------------------------------ */

    function newPin() {
        // Six digits, never starting with a zero so it reads cleanly aloud.
        var pin = String(1 + Math.floor(Math.random() * 9));
        while (pin.length < 6) pin += String(Math.floor(Math.random() * 10));
        return pin;
    }

    function initCrew() {
        var list = document.getElementById('crew-list');
        if (!list) return;

        var modal = document.getElementById('crew-modal');
        var editing = null;

        // A person's name sits in a different element depending on whether they are
        // a vendor, one of that vendor's staff, or somebody on a door.
        function nameOf(row) {
            var el = row.querySelector('.crew__name, .crewsub__name');
            return el ? el.textContent.trim() : 'them';
        }

        function metaOf(row) {
            var el = row.querySelector('.crew__meta, .crewsub__meta');
            return el ? el.textContent.replace(/\s+/g, ' ').trim() : '';
        }

        // The same person can be on several doors, so one PIN shows in several
        // places — a regenerate has to land on all of them.
        function pinCells(key) {
            return all('[data-crew-pin="' + key + '"]');
        }

        function recount() {
            // Only the filter hides rows here — the panel itself is hidden whenever
            // another hub tab is open, and that must not read as "nobody works here".
            var rows = all('[data-crew-row]').filter(function (row) {
                return !row.hidden && !row.closest('[data-crew-block][hidden]');
            });

            // Somebody on three doors appears three times, so people are counted by
            // who they are — their PIN key — not by how many cards they show up in.
            var seen = {};
            var vendors = 0;
            var food = 0;
            var entry = 0;

            rows.forEach(function (row) {
                var cell = row.querySelector('[data-crew-pin]');
                var key = cell ? cell.getAttribute('data-crew-pin') : null;
                if (!key || seen[key]) return;
                seen[key] = true;

                // A vendor's own staff are counted as food staff, not as vendors.
                if (row.getAttribute('data-crew-kind') !== 'vendor') entry += 1;
                else if (row.classList.contains('crewsub')) food += 1;
                else vendors += 1;
            });

            var people = vendors + food + entry;

            var count = document.querySelector('[data-crew-count]');
            var breakdown = document.querySelector('[data-crew-breakdown]');

            if (count) {
                count.textContent = people + ' ' + (people === 1 ? 'person' : 'people');
            }
            if (breakdown) {
                breakdown.textContent = vendors + ' vendor' + (vendors === 1 ? '' : 's') + ', '
                    + food + ' food staff, ' + entry + ' entry staff';
            }

            var empty = document.querySelector('[data-crew-empty]');
            if (empty) empty.hidden = people !== 0;
        }

        /* ---- filter by kind: whole blocks, so no empty heading is left behind ---- */
        var filter = document.getElementById('crewfilter');
        if (filter) {
            all('[data-pick]', filter).forEach(function (button) {
                button.addEventListener('click', function () {
                    var want = button.getAttribute('data-pick');

                    all('[data-crew-block]').forEach(function (block) {
                        block.hidden = want !== 'all'
                            && block.getAttribute('data-crew-block') !== want;
                    });

                    recount();
                });
            });
        }

        /* ---- regenerate a PIN ---- */
        all('[data-crew-regen]').forEach(function (button) {
            button.addEventListener('click', function () {
                var key = button.getAttribute('data-crew-regen');
                var cells = pinCells(key);
                if (!cells.length) return;

                var row = button.closest('[data-crew-row]');
                var name = nameOf(row);
                var fresh = newPin();

                cells.forEach(function (target) {
                    target.textContent = fresh;
                    target.classList.add('crew__pin--fresh');
                    window.setTimeout(function () {
                        target.classList.remove('crew__pin--fresh');
                    }, 1200);
                });

                toast('New PIN for <b>' + name + '</b> — the old one stopped working');
            });
        });

        /* ---- share a PIN ---- */
        all('[data-crew-share]').forEach(function (button) {
            button.addEventListener('click', function () {
                var key = button.getAttribute('data-crew-share');
                var row = button.closest('[data-crew-row]');
                var pin = document.querySelector('[data-crew-pin="' + key + '"]');

                function put(selector, text) {
                    var el = document.querySelector(selector);
                    if (el) el.textContent = text;
                }

                put('[data-share-name]', nameOf(row));
                put('[data-share-meta]', metaOf(row));
                put('[data-share-pin]', pin ? pin.textContent : '—');
            });
        });

        var sharesend = document.querySelector('[data-crew-sharesend]');
        if (sharesend) {
            sharesend.addEventListener('click', function () {
                var who = document.querySelector('[data-share-name]');
                toast('PIN sent to <b>' + (who ? who.textContent : 'them') + '</b>');
            });
        }

        /* ---- day ticks every slot on that day ---- */
        all('[data-crew-day]').forEach(function (dayBox) {
            dayBox.addEventListener('change', function () {
                var day = dayBox.getAttribute('data-crew-day');

                all('[data-crew-slot^="' + day + ':"]').forEach(function (slot) {
                    slot.checked = dayBox.checked;
                });
            });
        });

        all('[data-crew-slot]').forEach(function (slot) {
            slot.addEventListener('change', function () {
                var day = slot.getAttribute('data-crew-slot').split(':')[0];
                var siblings = all('[data-crew-slot^="' + day + ':"]');
                var dayBox = document.querySelector('[data-crew-day="' + day + '"]');
                if (!dayBox) return;

                var on = siblings.filter(function (s) { return s.checked; }).length;
                dayBox.checked = on === siblings.length;
                dayBox.indeterminate = on > 0 && on < siblings.length;
            });
        });

        /* ---- the detail field means different things per role ---- */
        var role = document.querySelector('[data-crew-field="role"]');
        var under = document.getElementById('crew-under');

        function applyRole() {
            var label = document.querySelector('[data-crew-detaillabel]');
            var detail = document.querySelector('[data-crew-field="detail"]');
            var vendor = role.value === 'Food vendor';
            var underVendor = role.value.indexOf('Food staff') === 0;

            if (label) {
                label.textContent = vendor ? 'Menu categories'
                    : underVendor ? 'Counter they run' : 'Post or door';
            }
            if (detail) {
                detail.placeholder = vendor ? 'e.g. Starters, Rice & Biryani'
                    : underVendor ? 'e.g. Atrium counter' : 'e.g. Hall A entrance';
            }

            // Only food staff report to a vendor, so the question is only asked of them.
            if (under) under.hidden = !underVendor;
        }

        if (role) {
            role.addEventListener('change', applyRole);
            applyRole();
        }

        // "Add staff" on a vendor card already knows the answer to both questions.
        all('[data-crew-under]').forEach(function (button) {
            button.addEventListener('click', function () {
                var vendorName = button.getAttribute('data-crew-under');
                var vendorField = document.querySelector('[data-crew-field="vendor"]');
                var heading = document.querySelector('[data-crew-heading]');
                var cta = document.querySelector('[data-crew-cta]');
                var newpin = document.querySelector('[data-crew-newpin]');

                ['name', 'mobile', 'detail'].forEach(function (key) {
                    var field = document.querySelector('[data-crew-field="' + key + '"]');
                    if (field) field.value = '';
                });

                all('[data-crew-slot], [data-crew-day]').forEach(function (box) {
                    box.checked = false;
                    box.indeterminate = false;
                });

                if (cta) cta.textContent = 'Add & generate PIN';
                if (newpin) newpin.textContent = '— — —';

                if (role) {
                    all('option', role).forEach(function (option) {
                        if (option.textContent.indexOf('Food staff') === 0) role.value = option.value;
                    });
                    applyRole();
                }

                if (vendorField) {
                    all('option', vendorField).forEach(function (option) {
                        if (option.textContent.trim() === vendorName) vendorField.value = option.value;
                    });
                }

                if (heading) heading.textContent = 'Add staff under ' + vendorName;
            });
        });

        /* ---- generate a PIN before saving ---- */
        var roll = document.querySelector('[data-crew-newpin-roll]');
        var pinField = document.querySelector('[data-crew-newpin]');
        if (roll && pinField) {
            roll.addEventListener('click', function () {
                pinField.textContent = newPin();
            });
        }

        /* ---- open the dialog fresh, or loaded with someone ---- */
        all('[data-crew-new]').forEach(function (button) {
            button.addEventListener('click', function () {
                editing = null;

                // A vendor's own "Add staff" button fills the dialog in itself, so the
                // blanket reset would only undo it.
                if (button.hasAttribute('data-crew-under')) return;

                function put(selector, text) {
                    var el = document.querySelector(selector);
                    if (el) el.textContent = text;
                }

                put('[data-crew-heading]', 'Add vendor or entry staff');
                put('[data-crew-cta]', 'Add & generate PIN');
                put('[data-crew-newpin]', '— — —');

                all('[data-crew-field]').forEach(function (field) {
                    if (field.tagName === 'SELECT') field.selectedIndex = 0;
                    else field.value = '';
                });

                all('[data-crew-slot], [data-crew-day]').forEach(function (box) {
                    box.checked = false;
                    box.indeterminate = false;
                });
            });
        });

        all('[data-crew-edit]').forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.closest('[data-crew-row]');
                editing = row;

                var name = nameOf(row);
                var detailEl = row.querySelector('.crew__detail');
                var detail = detailEl ? detailEl.textContent.trim() : '';
                var meta = metaOf(row);
                var key = button.getAttribute('data-crew-edit');
                var pin = document.querySelector('[data-crew-pin="' + key + '"]');

                function put(selector, text) {
                    var el = document.querySelector(selector);
                    if (el) el.textContent = text;
                }

                put('[data-crew-heading]', 'Edit ' + name);
                put('[data-crew-cta]', 'Save changes');
                put('[data-crew-newpin]', pin ? pin.textContent : '— — —');

                var nameField = document.querySelector('[data-crew-field="name"]');
                var mobileField = document.querySelector('[data-crew-field="mobile"]');
                var detailField = document.querySelector('[data-crew-field="detail"]');

                if (nameField) nameField.value = name;
                if (mobileField) mobileField.value = meta.split('·')[0].trim();
                if (detailField) detailField.value = detail;

                var scope = row.getAttribute('data-crew-scope');
                var pick = document.querySelector('[data-crew-scopepick="' + scope + '"]');
                if (pick) {
                    pick.checked = true;
                    pick.dispatchEvent(new Event('change', { bubbles: true }));
                }
            });
        });

        /* ---- save ---- */
        var save = document.querySelector('[data-crew-save]');
        if (save) {
            save.addEventListener('click', function () {
                var nameField = document.querySelector('[data-crew-field="name"]');
                var mobileField = document.querySelector('[data-crew-field="mobile"]');
                var name = nameField && nameField.value.trim();
                var mobile = mobileField && mobileField.value.trim();

                if (!name || !mobile) {
                    toast('A name and a mobile number are both needed', 'warn');
                    return;
                }

                var roleValue = role ? role.value : 'Food vendor';
                var detailField = document.querySelector('[data-crew-field="detail"]');
                var scopePick = all('[data-crew-scopepick]').filter(function (r) {
                    return r.checked;
                })[0];
                var wholeEvent = !scopePick || scopePick.getAttribute('data-crew-scopepick') === 'event';

                var slots = all('[data-crew-slot]').filter(function (box) { return box.checked; });

                if (!wholeEvent && !slots.length) {
                    toast('Pick at least one date or slot', 'warn');
                    return;
                }

                var pin = pinField && /^\d{6}$/.test(pinField.textContent)
                    ? pinField.textContent : newPin();

                if (editing) {
                    editing.querySelector('.crew__name').textContent = name;
                    editing.querySelector('.crew__meta').textContent = mobile + ' · ' + roleValue;
                    editing.querySelector('.crew__detail').textContent =
                        detailField ? detailField.value : '';
                    toast('<b>' + name + '</b> updated');
                    editing = null;
                    recount();
                    return;
                }

                // Clone an existing card so the new one inherits every hook and control.
                var pattern = all('[data-crew-row]', list)[0];
                if (!pattern) return;

                var key = 'crew-' + Date.now();
                var card = pattern.cloneNode(true);
                card.hidden = false;
                card.setAttribute('data-crew-key', key);
                card.setAttribute('data-crew-kind', roleValue === 'Food vendor' ? 'vendor' : 'staff');
                card.setAttribute('data-crew-scope', wholeEvent ? 'event' : 'slots');

                card.querySelector('.crew__name').textContent = name;
                card.querySelector('.crew__meta').textContent = mobile + ' · ' + roleValue;
                card.querySelector('.crew__detail').textContent =
                    detailField ? detailField.value : '';

                var pinEl = card.querySelector('[data-crew-pin]');
                pinEl.setAttribute('data-crew-pin', key);
                pinEl.textContent = pin;

                ['data-crew-regen', 'data-crew-share', 'data-crew-edit'].forEach(function (attr) {
                    var button = card.querySelector('[' + attr + ']');
                    if (button) button.setAttribute(attr, key);
                });

                var chips = card.querySelector('.assignchips');
                chips.textContent = '';

                if (wholeEvent) {
                    var all_ = document.createElement('span');
                    all_.className = 'assignchip assignchip--all';
                    all_.textContent = 'Whole event';
                    chips.appendChild(all_);
                } else {
                    slots.forEach(function (box) {
                        var label = box.closest('label').querySelector('.scopeslot__name');
                        var chip = document.createElement('span');
                        chip.className = 'assignchip';
                        chip.textContent = label ? label.textContent.trim() : 'Slot';
                        chips.appendChild(chip);
                    });
                }

                list.appendChild(card);
                document.dispatchEvent(new CustomEvent('planora:rekey', { detail: card }));
                recount();

                toast('<b>' + name + '</b> added with PIN <b>' + pin + '</b> — '
                    + (wholeEvent ? 'whole event' : slots.length + ' slot'
                        + (slots.length === 1 ? '' : 's')));
            });
        }

        document.addEventListener('planora:rowsChanged', recount);
        recount();
    }

    /* ------------------------------------------------------------------------
       56. Table search and column filter
       --------------------------------------------------------------------------
       A search box and a category select that narrow a table in place, with an
       empty state when nothing matches. Generic so any table can use it.
       ------------------------------------------------------------------------ */

    function initTableFilter() {
        var seen = {};

        function bind(tableId) {
            if (seen[tableId]) return;
            seen[tableId] = true;

            var table = document.getElementById(tableId);
            if (!table) return;

            var text = document.querySelector('[data-tablefilter="' + tableId + '"]');
            var col = document.querySelector('[data-tablefilter-col="' + tableId + '"]');
            var empty = document.querySelector('[data-tablefilter-empty="' + tableId + '"]');

            function apply() {
                var needle = text ? text.value.trim().toLowerCase() : '';
                var wanted = col ? col.value.trim().toLowerCase() : '';
                var shown = 0;

                all('tbody tr', table).forEach(function (row) {
                    var haystack = row.textContent.toLowerCase();
                    var keep = (!needle || haystack.indexOf(needle) !== -1)
                        && (!wanted || haystack.indexOf(wanted) !== -1);

                    row.hidden = !keep;
                    if (keep) shown += 1;
                });

                if (empty) empty.hidden = shown !== 0;
            }

            if (text) text.addEventListener('input', apply);
            if (col) col.addEventListener('change', apply);
            apply();
        }

        all('[data-tablefilter]').forEach(function (el) {
            bind(el.getAttribute('data-tablefilter'));
        });
        all('[data-tablefilter-col]').forEach(function (el) {
            bind(el.getAttribute('data-tablefilter-col'));
        });
    }

    /* ------------------------------------------------------------------------
       57. Food order stages
       --------------------------------------------------------------------------
       An order moves New -> Preparing -> Ready -> Served. The button advances one
       step and relabels itself, and the last step becomes a collected badge.
       ------------------------------------------------------------------------ */

    var ORDER_STAGES = ['New', 'Preparing', 'Ready', 'Served'];
    var ORDER_CLASS = { New: 'new', Preparing: 'paused', Ready: 'ready', Served: 'draft' };

    function initOrderStages() {
        document.addEventListener('click', function (event) {
            var button = event.target.closest
                ? event.target.closest('[data-order-advance]') : null;
            if (!button) return;

            var row = button.closest('[data-order-row]');
            if (!row) return;

            var next = button.getAttribute('data-order-advance');
            var badge = row.querySelector('[data-order-badge]');
            var ref = row.querySelector('.order__ref');

            if (badge) {
                badge.textContent = next;
                badge.className = 'badge badge--nodot badge--' + (ORDER_CLASS[next] || 'draft');
                badge.setAttribute('data-order-badge', '');
            }

            row.setAttribute('data-order-stage', next);

            var after = ORDER_STAGES[ORDER_STAGES.indexOf(next) + 1];

            if (after) {
                button.setAttribute('data-order-advance', after);
                button.textContent = 'Mark ' + after;
            } else {
                var done = document.createElement('span');
                done.className = 'badge badge--nodot badge--live';
                done.textContent = 'Collected';
                button.parentNode.replaceChild(done, button);
            }

            toast('Order <b>' + (ref ? ref.textContent : '') + '</b> is now ' + next);
        });
    }

    /* ------------------------------------------------------------------------
       58. Adding and editing a menu item
     59. Registration gate + submissions
     60. Door scanner and its verdict
     61. Communication credit packs
       --------------------------------------------------------------------------
       Items have a name, a category, a dietary type and how many extra serves a
       guest may take. No price — food is included, not sold.
       ------------------------------------------------------------------------ */

    function initFoodItems() {
        var table = document.getElementById('food-table');
        if (!table) return;

        var editing = null;

        function field(name) {
            return document.querySelector('[data-food-field="' + name + '"]');
        }

        all('[data-food-new]').forEach(function (button) {
            button.addEventListener('click', function () {
                editing = null;

                var heading = document.querySelector('[data-food-heading]');
                var cta = document.querySelector('[data-food-cta]');
                if (heading) heading.textContent = 'Add menu item';
                if (cta) cta.textContent = 'Add to menu';

                ['name', 'cat', 'diet', 'extra'].forEach(function (key) {
                    var input = field(key);
                    if (!input) return;
                    if (input.tagName === 'SELECT') input.selectedIndex = 0;
                    else input.value = key === 'extra' ? '1' : '';
                });
            });
        });

        all('[data-food-edit]').forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.closest('[data-food-row]');
                editing = row;

                var name = row.querySelector('.cellstack__name').textContent.trim();
                var cat = row.querySelector('.badge--neutral').textContent.trim();
                var extra = row.querySelector('.stepper__value');

                var heading = document.querySelector('[data-food-heading]');
                var cta = document.querySelector('[data-food-cta]');
                if (heading) heading.textContent = 'Edit ' + name;
                if (cta) cta.textContent = 'Save changes';

                if (field('name')) field('name').value = name;
                if (field('cat')) field('cat').value = cat;
                if (field('extra')) field('extra').value = extra ? extra.value : '1';
            });
        });

        var save = document.querySelector('[data-food-save]');
        if (!save) return;

        save.addEventListener('click', function () {
            var name = field('name') ? field('name').value.trim() : '';
            if (!name) {
                toast('The item needs a name', 'warn');
                return;
            }

            var cat = field('cat') ? field('cat').value : '';
            var diet = field('diet') ? field('diet').value : 'veg';
            var dietLabel = field('diet')
                ? field('diet').options[field('diet').selectedIndex].text : 'Veg';
            var extra = field('extra') ? field('extra').value : '1';

            if (editing) {
                editing.querySelector('.cellstack__name').textContent = name;
                editing.querySelector('.badge--neutral').textContent = cat;

                var dot = editing.querySelector('.diet');
                if (dot) dot.className = 'diet diet--' + diet;

                var sub = editing.querySelector('.cellsub');
                if (sub) sub.innerHTML = '<span class="diet diet--' + diet
                    + '" aria-hidden="true"></span> ' + dietLabel;

                var step = editing.querySelector('.stepper__value');
                if (step) step.value = extra;

                toast('<b>' + name + '</b> updated');
                editing = null;
                return;
            }

            var body = table.querySelector('tbody');
            var pattern = body.querySelector('[data-food-row]');
            if (!pattern) return;

            var row = pattern.cloneNode(true);
            row.hidden = false;
            body.appendChild(row);
            document.dispatchEvent(new CustomEvent('planora:rekey', { detail: row }));

            row.querySelector('.cellstack__name').textContent = name;
            row.querySelector('.badge--neutral').textContent = cat;
            row.querySelector('.cellsub').innerHTML = '<span class="diet diet--' + diet
                + '" aria-hidden="true"></span> ' + dietLabel;
            row.querySelector('.stepper__value').value = extra;

            var served = row.querySelector('td:nth-child(4)');
            if (served) served.innerHTML = '<b>0</b> <span class="cellsub">today</span>';

            var avail = row.querySelector('[data-availability]');
            if (avail) {
                avail.textContent = 'Available';
                avail.className = 'badge badge--live';
                avail.setAttribute('aria-pressed', 'true');
            }

            toast('<b>' + name + '</b> added to the menu');
        });
    }

    /* ------------------------------------------------------------------------
       59. The registration gate, and reading the submissions
       --------------------------------------------------------------------------
       Access Control decides whether guest details are collected at all. That
       answer travels to Event Hub the same way the event type and the RSVP answer
       do, and it decides whether the Registrations tab is usable or locked.
  
       In the tab itself: a completeness filter alongside the shared table search,
       a drawer showing one guest's whole submission, and an export count that
       follows whatever is currently filtered.
       ------------------------------------------------------------------------ */

    function initRegGate() {
        // Wizard side: carry the answer into the hub link.
        var flags = all('[data-reg-flag]');
        if (flags.length) {
            function syncFlag() {
                var picked = flags.filter(function (f) { return f.checked; })[0];
                var flag = picked ? picked.getAttribute('data-reg-flag') : 'on';

                all('[data-reg-link]').forEach(function (link) {
                    link.setAttribute('href',
                        link.getAttribute('href').replace(/reg=(on|off)/, 'reg=' + flag));
                });
            }

            flags.forEach(function (flag) {
                flag.addEventListener('change', syncFlag);
            });
            syncFlag();
        }

        // Hub side: lock the tab when registration was switched off.
        var tab = document.querySelector('[data-reg-tab]');
        if (!tab) return;

        var off = /[?&]reg=off/.test(window.location.search);
        var locked = document.querySelector('[data-reg-locked]');
        var live = document.querySelector('[data-reg-live]');

        if (locked) locked.hidden = !off;
        if (live) live.hidden = off;

        tab.disabled = off;
        tab.setAttribute('aria-disabled', String(off));
        tab.classList.toggle('subtabs__tab--locked', off);

        if (off) tab.title = 'Guest registration is switched off for this event';
        else tab.removeAttribute('title');
    }

    var REG_LABELS = ['Phone number', 'Organisation', 'Job title',
        'Dietary requirements', 'Ticket tier', 'Submitted'];

    function initRegistrations() {
        var table = document.getElementById('reg-table');
        if (!table) return;

        /* ---- completeness filter, on top of the shared search ---- */
        var state = document.querySelector('[data-regstate]');

        function applyState() {
            var want = state ? state.value : '';

            all('[data-reg-row]', table).forEach(function (row) {
                if (want && row.getAttribute('data-reg-state') !== want) {
                    row.hidden = true;
                }
            });

            countExport();
        }

        function countExport() {
            var shown = all('[data-reg-row]', table).filter(function (row) {
                return !row.hidden;
            }).length;

            var target = document.querySelector('[data-regexport-count]');
            if (target) {
                target.textContent = shown + ' registration' + (shown === 1 ? '' : 's');
            }
        }

        if (state) {
            state.addEventListener('change', function () {
                // Let the shared filter run first, then narrow further.
                all('[data-reg-row]', table).forEach(function (row) { row.hidden = false; });

                var search = document.querySelector('[data-tablefilter="reg-table"]');
                var tier = document.querySelector('[data-tablefilter-col="reg-table"]');
                if (search) search.dispatchEvent(new Event('input', { bubbles: true }));
                if (tier) tier.dispatchEvent(new Event('change', { bubbles: true }));

                applyState();
            });
        }

        ['[data-tablefilter="reg-table"]', '[data-tablefilter-col="reg-table"]']
            .forEach(function (selector) {
                var el = document.querySelector(selector);
                if (!el) return;
                el.addEventListener('input', applyState);
                el.addEventListener('change', applyState);
            });

        /* ---- one guest's whole submission ---- */
        all('[data-reg-view]').forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.closest('[data-reg-row]');
                var cells = all('td', row);

                var name = row.querySelector('.cellstack__name').textContent.trim();
                var email = row.querySelector('.cellsub').textContent.trim();

                var heading = document.querySelector('[data-regdetail-name]');
                var meta = document.querySelector('[data-regdetail-meta]');
                var list = document.querySelector('[data-regdetail-list]');
                if (!list) return;

                if (heading) heading.textContent = name;
                if (meta) {
                    meta.textContent = email + ' · '
                        + row.getAttribute('data-reg-tier') + ' · '
                        + (row.getAttribute('data-reg-state') === 'complete'
                            ? 'complete' : 'missing answers');
                }

                list.textContent = '';

                REG_LABELS.forEach(function (label, i) {
                    var cell = cells[i + 1];
                    if (!cell) return;

                    var dt = document.createElement('dt');
                    dt.textContent = label;

                    var dd = document.createElement('dd');
                    var value = cell.textContent.trim();
                    dd.textContent = value;
                    if (/Not answered/.test(value)) dd.className = 'regmiss';

                    list.appendChild(dt);
                    list.appendChild(dd);
                });
            });
        });

        countExport();
    }

    /* ------------------------------------------------------------------------
       60. The door scanner and its verdict
       --------------------------------------------------------------------------
       Three answers matter at a door: let them in, they are already in, or this
       pass is not for today. The scanner shows one of those and nothing else, and
       the day tabs re-scope the whole page.
       ------------------------------------------------------------------------ */

    var PASSES = {
        'TSA-4821': {
            name: 'Amara Osei', meta: 'VIP · Seat A-06 · Day 1',
            verdict: 'in', reason: 'Valid for today. Let them in.'
        },
        'TSA-4820': {
            name: 'Tunde Alabi', meta: 'Standard · Seat C-14 · Day 1',
            verdict: 'in', reason: 'Valid for today. Let them in.'
        },
        'TSA-0000': {
            name: 'Grace Mensah', meta: 'Standard · Seat B-02 · Day 2 only',
            verdict: 'denied',
            reason: 'This pass covers Day 2. It is not valid today.'
        },
        'TSA-4817': {
            name: 'Ngozi Eze', meta: 'Standard · Seat D-09 · Day 1',
            verdict: 'repeat',
            reason: 'Already checked in at 09:33 on the Main Hall entrance.'
        }
    };

    var VERDICT_LOOK = {
        in: { cls: 'verdict--in', mark: '✓', label: 'Checked in' },
        denied: { cls: 'verdict--denied', mark: '✕', label: 'Denied' },
        repeat: { cls: 'verdict--repeat', mark: '!', label: 'Already in' }
    };

    function initScanner() {
        // There is no camera step any more: check-in is a typed ticket reference,
        // which is the only method a host can rely on across every device at a
        // door. So this hangs off the field rather than off a scanner box.
        var field = document.querySelector('[data-scan-ref]');
        if (!field) return;

        var verdict = document.querySelector('[data-verdict]');

        function show(pass) {
            if (!verdict) return;

            var look = VERDICT_LOOK[pass.verdict];

            verdict.hidden = false;
            verdict.className = 'verdict ' + look.cls;

            var mark = verdict.querySelector('[data-verdict-mark]');
            var name = verdict.querySelector('[data-verdict-name]');
            var meta = verdict.querySelector('[data-verdict-meta]');
            var reason = verdict.querySelector('[data-verdict-reason]');

            if (mark) mark.textContent = look.mark;
            if (name) name.textContent = pass.name;
            if (meta) meta.textContent = pass.meta;
            if (reason) reason.textContent = pass.reason;

            toast('<b>' + pass.name + '</b> — ' + look.label,
                pass.verdict === 'in' ? '' : 'warn');
        }

        function lookup() {
            var ref = field.value.trim().toUpperCase();
            if (!ref) {
                toast('Enter a ticket reference first', 'warn');
                return;
            }

            var pass = PASSES[ref];

            if (!pass) {
                show({
                    name: 'Unknown pass', meta: ref,
                    verdict: 'denied',
                    reason: 'No ticket on this event carries that reference.'
                });
                return;
            }

            show(pass);
            field.value = '';
        }

        var button = document.querySelector('[data-scan-lookup]');
        if (button) button.addEventListener('click', lookup);

        // Enter is how somebody at a door actually submits — one hand on the
        // keyboard, eyes on the guest.
        field.addEventListener('keydown', function (event) {
            if (event.key !== 'Enter') return;
            event.preventDefault();
            lookup();
        });

        // Day tabs re-scope the page.
        all('[data-checkin-day]').forEach(function (tab) {
            tab.addEventListener('click', function () {
                all('[data-checkin-day]').forEach(function (other) {
                    other.setAttribute('aria-pressed', String(other === tab));
                });

                var label = tab.textContent.replace(/\s+/g, ' ').trim().split('—')[0].trim();
                all('[data-scope-label]').forEach(function (el) { el.textContent = label; });

                toast('Showing <b>' + label + '</b>');
            });
        });
    }

    /* ------------------------------------------------------------------------
       61. Buying communication credits
       --------------------------------------------------------------------------
       Credits are bought per channel in packs of 10 or 100, and the 100 pack is
       cheaper per credit. So the arithmetic has to be visible: credits added, the
       new balance, the cost per channel and the total, all recomputed as the host
       nudges the steppers.
       ------------------------------------------------------------------------ */

    function initCreditPacks() {
        var cards = all('[data-creditcard]');
        if (!cards.length) return;

        function naira(value) {
            return '₦' + value.toLocaleString('en-US');
        }

        function render() {
            var total = 0;

            cards.forEach(function (card) {
                var per10 = parseInt(card.getAttribute('data-per10'), 10);
                var per100 = parseInt(card.getAttribute('data-per100'), 10);

                var ten = parseInt(card.querySelector('[data-pack="10"]').value, 10) || 0;
                var hundred = parseInt(card.querySelector('[data-pack="100"]').value, 10) || 0;

                var credits = ten * 10 + hundred * 100;
                var cost = ten * per10 + hundred * per100;
                total += cost;

                var balance = parseInt(
                    card.querySelector('.creditcard__value').textContent.replace(/\D/g, ''), 10);

                var add = card.querySelector('[data-credit-add]');
                var next = card.querySelector('[data-credit-new]');
                var costEl = card.querySelector('[data-credit-cost]');

                if (add) add.textContent = credits.toLocaleString('en-US');
                if (next) next.textContent = (balance + credits).toLocaleString('en-US');
                if (costEl) costEl.textContent = naira(cost);

                card.classList.toggle('is-buying', credits > 0);

                var line = document.querySelector('[data-total-'
                    + card.getAttribute('data-creditcard') + ']');
                if (line) {
                    line.textContent = credits.toLocaleString('en-US') + ' credits · ' + naira(cost);
                }
            });

            var totalEl = document.querySelector('[data-total-cost]');
            if (totalEl) totalEl.textContent = naira(total);

            var submit = document.querySelector('[data-topup-submit]');
            var cta = document.querySelector('[data-topup-cta]');

            if (submit) submit.disabled = total === 0;
            if (cta) cta.textContent = total ? 'Pay ' + naira(total) : 'Add credits';
        }

        all('[data-pack]').forEach(function (input) {
            input.addEventListener('input', render);
            input.addEventListener('change', render);
        });

        // The steppers change the value without firing input, so listen for clicks too.
        all('[data-creditcard] [data-step]').forEach(function (button) {
            button.addEventListener('click', function () {
                window.setTimeout(render, 0);
            });
        });

        render();
    }

    /* ------------------------------------------------------------------------
       Bootstrap
       ------------------------------------------------------------------------ */

    /* ------------------------------------------------------------------------
       62. Record picker — one page, many records
       --------------------------------------------------------------------------
       A refund receipt is the same layout for every refund, so the page holds all
       of them and the link decides which one is on screen. Arriving from Sales &
       Revenue carries the reference in the query string; the select then switches
       records without leaving the page.
       ---------------------------------------------------------------------- */
    function initDocPick() {
        var pick = all('[data-docpick]')[0];
        if (!pick) return;

        var docs = all('[data-doc]');
        if (!docs.length) return;

        function show(ref) {
            var found = false;

            docs.forEach(function (doc) {
                var match = doc.getAttribute('data-doc') === ref;
                doc.hidden = !match;
                if (match) found = true;
            });

            // An unknown or missing reference falls back to the first record rather
            // than leaving the page blank.
            if (!found) {
                docs[0].hidden = false;
                pick.value = docs[0].getAttribute('data-doc');
            }
        }

        var param = pick.getAttribute('data-docpick');
        var wanted = new URLSearchParams(window.location.search).get(param);
        if (wanted) pick.value = wanted;

        show(pick.value);

        pick.addEventListener('change', function () { show(pick.value); });
    }

    /* ------------------------------------------------------------------------
       63. Seat layout jump
       --------------------------------------------------------------------------
       A tier card says which layout it belongs to, so "View layout" switches to
       that layout and scrolls to it rather than making the host find it.
       ---------------------------------------------------------------------- */
    function initSeatJump() {
        var jumps = all('[data-seatjump]');
        if (!jumps.length) return;

        jumps.forEach(function (button) {
            button.addEventListener('click', function () {
                var want = button.getAttribute('data-seatjump');
                var group = document.getElementById('seatlayout');
                if (!group) return;

                var opt = group.querySelector('[data-pick="' + want + '"]');
                if (opt) opt.click();

                var panel = document.querySelector('[data-pick-panel="' + want + '"]');
                if (panel) panel.scrollIntoView({ behavior: 'smooth', block: 'start' });
            });
        });
    }

    /* ------------------------------------------------------------------------
       64. Bank account lookup
       --------------------------------------------------------------------------
       Nobody types their own account name correctly under pressure, and a wrong
       name is how a payout goes to a stranger. So the number is the input and the
       name comes back from the bank — resolved once all ten digits are in, and
       reset the moment the number changes again.
       ---------------------------------------------------------------------- */
    var BANK_NAMES = {
        '0123456789': 'TECHSPHERE EVENTS LIMITED',
        '2244668800': 'KOFI ASANTE',
        '9988776655': 'TECHSPHERE MEDIA LIMITED'
    };

    function initBankLookup() {
        all('[data-bank-lookup-no]').forEach(function (field) {
            var scope = field.closest('[data-modal], form, body');
            var out = scope.querySelector('[data-bank-lookup-out]');
            var name = scope.querySelector('[data-bank-lookup-name]');
            var bank = scope.querySelector('[data-bank-lookup-bank]');
            if (!name) return;

            function resolve() {
                var digits = field.value.replace(/\D/g, '').slice(0, 10);
                if (field.value !== digits) field.value = digits;

                if (digits.length < 10) {
                    if (out) out.classList.add('is-waiting');
                    name.textContent = digits.length
                        ? (10 - digits.length) + ' more digit'
                        + (10 - digits.length === 1 ? '' : 's') + ' needed'
                        : 'Waiting for a 10-digit number';
                    return;
                }

                if (out) out.classList.remove('is-waiting');
                name.textContent = 'Checking with '
                    + (bank ? bank.value.split('&')[0].trim() : 'the bank') + '…';

                window.setTimeout(function () {
                    // An unknown number still resolves — a real lookup would return
                    // whatever the bank holds, not a blank.
                    name.textContent = BANK_NAMES[digits] || 'TECHSPHERE EVENTS LIMITED';
                    toast('Account name fetched from the bank — check it matches');
                }, 600);
            }

            field.addEventListener('input', resolve);
            if (bank) bank.addEventListener('change', resolve);
            resolve();
        });

        // Editing an existing account names it in the dialog title.
        all('[data-bank-edit]').forEach(function (button) {
            button.addEventListener('click', function () {
                var row = button.closest('[data-bank-row]');
                var heading = document.querySelector('[data-bank-heading]');
                var bankName = row ? row.querySelector('.bankcard__bank') : null;

                if (heading) {
                    heading.textContent = bankName
                        ? 'Edit ' + bankName.textContent.trim() + ' account'
                        : 'Edit payout account';
                }
            });
        });

        all('[data-bank-new]').forEach(function (button) {
            button.addEventListener('click', function () {
                var heading = document.querySelector('[data-bank-heading]');
                if (heading) heading.textContent = 'Add a payout account';
            });
        });
    }

    /* ------------------------------------------------------------------------
       65. Buy-credits jump
       --------------------------------------------------------------------------
       Each balance card offers "Buy Credits". Following it should not just move the
       page — it should arrive with the right channel already selected, because the
       host asked for WhatsApp, not for a form.
       ---------------------------------------------------------------------- */
    function initBuyCredits() {
        var links = all('[data-buycredits]');
        if (!links.length) return;

        var channel = document.querySelector('[data-buycredits-channel]');

        links.forEach(function (link) {
            link.addEventListener('click', function () {
                var want = link.getAttribute('data-buycredits');

                if (channel) {
                    all('option', channel).forEach(function (option) {
                        if (option.textContent.trim() === want) channel.value = option.value;
                    });
                }

                var panel = document.getElementById('buy-credits');
                if (panel) panel.scrollIntoView({ behavior: 'smooth', block: 'start' });
            });
        });
    }

    /* ------------------------------------------------------------------------
       66. Ticketed vs non-ticketed gating
       --------------------------------------------------------------------------
       Public/private already rides in the query string. Whether a public event
       actually sells tickets is a second, independent question, and it is the one
       that decides whether money changes hands — so it gets its own flag
       (?sale=ticketed|free) and its own attributes. A private event is one mode
       regardless of the flag, because a private event is never commissioned even
       when it issues tickets.
  
         [data-mode-only="public-ticketed private"]  shown only in those modes
         [data-mode-hide="public-free"]              hidden in those modes
         [data-mode-label]                           prints "ticketed" / "non-ticketed"
       ---------------------------------------------------------------------- */

    var MODE_WORDS = {
        'public-ticketed': 'ticketed',
        'public-free': 'non-ticketed',
        'private': 'invitation only'
    };

    function currentMode() {
        if (currentType() === 'private') return 'private';

        var match = /[?&]sale=(ticketed|free)/.exec(window.location.search);
        return (match && match[1] === 'free') ? 'public-free' : 'public-ticketed';
    }

    function initSaleMode() {
        var mode = currentMode();
        document.body.setAttribute('data-sale-mode', mode);

        function listed(value) {
            return value ? value.split(/\s+/) : [];
        }

        all('[data-mode-only]').forEach(function (el) {
            el.hidden = listed(el.getAttribute('data-mode-only')).indexOf(mode) === -1;
        });

        all('[data-mode-hide]').forEach(function (el) {
            el.hidden = listed(el.getAttribute('data-mode-hide')).indexOf(mode) !== -1;
        });

        all('[data-mode-label]').forEach(function (el) {
            el.textContent = MODE_WORDS[mode];
        });

        // Keep the flag on hops between the pages that care about it. Wizard links
        // carry it too, because whether the Payout step exists depends on it.
        all('[data-mode-link], [data-wizard-link]').forEach(function (link) {
            var href = link.getAttribute('href');
            if (!href || href.indexOf('sale=') !== -1) return;

            link.setAttribute('href', href + (href.indexOf('?') === -1 ? '?' : '&')
                + 'sale=' + (mode === 'public-free' ? 'free' : 'ticketed'));
        });
    }

    /* ------------------------------------------------------------------------
       67. Add capacity to a published event
       --------------------------------------------------------------------------
       The host enters a headcount. Everything else is derived: the split across
       the zones already configured (proportional to what each zone already holds),
       the whole units that implies for row and table zones, the ticket value of
       the new seats, and the commission — which is 2% for a public ticketed event
       and nothing at all otherwise.
       ---------------------------------------------------------------------- */

    function initAddCapacity() {
        var flow = document.querySelector('[data-capflow]');
        if (!flow) return;

        var zones = all('[data-capzone]', flow).map(function (el) {
            return {
                el: el,
                key: el.getAttribute('data-capzone'),
                mode: el.getAttribute('data-cap-mode'),
                per: parseInt(el.getAttribute('data-cap-per'), 10) || 1,
                now: parseInt(el.getAttribute('data-cap-now'), 10) || 0,
                price: parseInt(el.getAttribute('data-cap-price').replace(/\D/g, ''), 10) || 0,
                name: el.querySelector('.capzone__name').textContent.trim(),
                tier: el.querySelector('.capzone__meta').textContent.split('·')[0].trim(),
                added: 0
            };
        });

        var countField = flow.querySelector('[data-cap-count]');
        var next = flow.querySelector('[data-cap-next]');
        var back = flow.querySelector('[data-cap-back]');
        var done = flow.querySelector('[data-cap-done]');
        var cancel = flow.querySelector('[data-cap-cancel]');
        var nextLabel = flow.querySelector('[data-cap-nextlabel]');
        var step = 1;
        var manual = false;

        var LABELS = { 1: 'Preview seating', 2: 'See the charge', 3: 'Confirm and add' };
        var totalNow = zones.reduce(function (sum, z) { return sum + z.now; }, 0);

        function money(value) {
            return '₦' + value.toLocaleString('en-US');
        }

        function requested() {
            return Math.max(0, parseInt(countField.value.replace(/\D/g, ''), 10) || 0);
        }

        function allocated() {
            return zones.reduce(function (sum, z) { return sum + z.added; }, 0);
        }

        /* ---- the split: proportional to what each zone already holds ---- */
        function autoSplit() {
            var want = requested();

            zones.forEach(function (z) {
                var share = totalNow ? want * (z.now / totalNow) : 0;

                // Rows and tables are built whole, so a part-unit rounds up. Standing and
                // open seating have no unit, so they take their share as headcount.
                z.added = (z.mode === 'rows' || z.mode === 'tables')
                    ? Math.ceil(share / z.per) * z.per
                    : Math.round(share);
            });

            // Rounding up per zone can overshoot badly; give back whole units from the
            // largest zones until the overshoot is under one unit each.
            var over = allocated() - want;
            zones.slice().sort(function (a, b) { return b.per - a.per; }).forEach(function (z) {
                if (z.mode !== 'rows' && z.mode !== 'tables') return;
                while (over >= z.per && z.added >= z.per) {
                    z.added -= z.per;
                    over -= z.per;
                }
            });
        }

        /* ---- paint every derived number ---- */
        function render() {
            var want = requested();
            var alloc = allocated();

            all('[data-cap-added]', flow).forEach(function (el) {
                el.textContent = want.toLocaleString('en-US');
            });
            all('[data-cap-alloc]', flow).forEach(function (el) {
                el.textContent = alloc.toLocaleString('en-US');
            });
            all('[data-cap-newtotal]', flow).forEach(function (el) {
                el.textContent = (totalNow + (step === 1 ? want : alloc)).toLocaleString('en-US');
            });

            zones.forEach(function (z) {
                var seats = z.el.querySelector('[data-cap-seats="' + z.key + '"]');
                var units = z.el.querySelector('[data-cap-units="' + z.key + '"]');
                var spare = z.el.querySelector('[data-cap-spare="' + z.key + '"]');
                var vis = z.el.querySelector('[data-cap-vis="' + z.key + '"]');
                var adjust = z.el.querySelector('[data-cap-adjust="' + z.key + '"]');

                if (seats) seats.textContent = z.added.toLocaleString('en-US');
                if (adjust && String(z.added) !== adjust.value) adjust.value = z.added;
                z.el.classList.toggle('is-empty', z.added === 0);

                if (units) units.textContent = Math.round(z.added / z.per);
                if (spare) {
                    spare.textContent = z.added
                        ? '= ' + z.added + ' seats'
                        : 'nothing added here';
                }
                if (vis) {
                    var chips = '';
                    var count = Math.min(Math.round(z.added / z.per), 12);
                    for (var i = 0; i < count; i += 1) chips += '<span class="capunit"></span>';
                    if (Math.round(z.added / z.per) > 12) {
                        chips += '<span class="capunit capunit--more">+'
                            + (Math.round(z.added / z.per) - 12) + '</span>';
                    }
                    vis.innerHTML = chips;
                }
            });

            var tally = flow.querySelector('[data-cap-tallystate]');
            if (tally) {
                tally.textContent = !alloc ? 'Nothing allocated yet'
                    : alloc === want ? 'Matches the headcount exactly'
                        : alloc > want ? (alloc - want) + ' seats of headroom from whole rows and tables'
                            : (want - alloc) + ' seats still to place';
                if (manual) tally.textContent += ' · adjusted by hand';
                tally.className = 'captally__state'
                    + (alloc < want ? ' captally__state--short' : '');
            }

            renderCharges();
            renderReceipt();

            if (next) next.disabled = step === 1 ? want === 0 : alloc === 0;
        }

        function renderCharges() {
            var body = flow.querySelector('[data-cap-charges]');
            if (!body) return;

            var gross = 0;
            var rows = '';

            zones.forEach(function (z) {
                if (!z.added) return;
                var value = z.added * z.price;
                gross += value;

                rows += '<tr><th scope="row">' + z.name + '</th>'
                    + '<td>' + z.tier + '</td>'
                    + '<td>' + z.added + '</td>'
                    + '<td><span class="u-nowrap">' + money(z.price) + '</span></td>'
                    + '<td><span class="u-nowrap">' + money(value) + '</span></td>'
                    + '<td><span class="u-nowrap">' + money(Math.round(value * 0.02))
                    + '</span></td></tr>';
            });

            body.innerHTML = rows
                || '<tr><td colspan="6" class="u-muted">Nothing added yet</td></tr>';

            var g = flow.querySelector('[data-cap-gross]');
            var c = flow.querySelector('[data-cap-comm]');
            if (g) g.textContent = money(gross);
            if (c) c.textContent = money(Math.round(gross * 0.02));
        }

        function renderReceipt() {
            var body = flow.querySelector('[data-cap-receipt]');
            if (!body) return;

            var rows = '';
            zones.forEach(function (z) {
                if (!z.added) return;

                var built = (z.mode === 'rows' || z.mode === 'tables')
                    ? Math.round(z.added / z.per) + (z.mode === 'rows' ? ' rows' : ' tables')
                    + ' × ' + z.per + ' seats'
                    : z.added + ' guests of headroom';

                rows += '<tr><th scope="row">' + z.name + '</th>'
                    + '<td class="u-muted">' + z.el.querySelector('.capzone__meta')
                        .textContent.split('·').pop().trim() + '</td>'
                    + '<td>' + built + '</td>'
                    + '<td>' + (z.now + z.added) + ' seats</td></tr>';
            });

            body.innerHTML = rows
                || '<tr><td colspan="4" class="u-muted">Nothing added</td></tr>';
        }

        /* ---- step movement ---- */
        function show(target) {
            step = target;

            all('[data-capstep]', flow).forEach(function (panel) {
                panel.hidden = panel.getAttribute('data-capstep') !== String(target);
            });

            all('[data-capstep-dot]', flow).forEach(function (dot) {
                var n = parseInt(dot.getAttribute('data-capstep-dot'), 10);
                dot.classList.toggle('is-current', n === target);
                dot.classList.toggle('is-done', n < target);
            });

            if (back) back.hidden = target === 1 || target === 4;
            if (cancel) cancel.hidden = target === 4;
            if (next) next.hidden = target === 4;
            if (done) done.hidden = target !== 4;
            if (nextLabel && LABELS[target]) nextLabel.textContent = LABELS[target];

            render();
        }

        if (countField) {
            countField.addEventListener('input', function () {
                manual = false;
                autoSplit();
                render();
            });
        }

        // The steppers change the value without firing input.
        all('[data-stepper] [data-step]', flow).forEach(function (button) {
            button.addEventListener('click', function () {
                window.setTimeout(function () {
                    if (button.closest('.capzone')) {
                        manual = true;
                        zones.forEach(function (z) {
                            var adjust = z.el.querySelector('[data-cap-adjust="' + z.key + '"]');
                            if (adjust) z.added = parseInt(adjust.value.replace(/\D/g, ''), 10) || 0;
                        });
                    } else {
                        manual = false;
                        autoSplit();
                    }
                    render();
                }, 0);
            });
        });

        all('[data-cap-quick]', flow).forEach(function (chip) {
            chip.addEventListener('click', function () {
                countField.value = requested()
                    + parseInt(chip.getAttribute('data-cap-quick'), 10);
                manual = false;
                autoSplit();
                render();
            });
        });

        all('[data-cap-adjust]', flow).forEach(function (field) {
            field.addEventListener('input', function () {
                manual = true;
                var zone = zones.filter(function (z) {
                    return z.key === field.getAttribute('data-cap-adjust');
                })[0];
                if (zone) zone.added = parseInt(field.value.replace(/\D/g, ''), 10) || 0;
                render();
            });
        });

        var reset = flow.querySelector('[data-cap-reset]');
        if (reset) {
            reset.addEventListener('click', function () {
                manual = false;
                autoSplit();
                render();
                toast('Split recalculated from the layout you already configured');
            });
        }

        if (next) {
            next.addEventListener('click', function () {
                if (step === 3) {
                    var mode = currentMode();
                    show(4);

                    var note = flow.querySelector('[data-cap-donetext]');
                    if (note) {
                        note.textContent = mode === 'public-ticketed'
                            ? 'The extra seats are on sale. Commission at 2% is taken only on the ones '
                            + 'that sell.'
                            : mode === 'private'
                                ? 'Invitations and passes are on their way. Nothing was charged.'
                                : 'Added at no charge under your enterprise plan.';
                    }

                    toast('<b>' + allocated() + ' seats</b> added — the layout was extended, '
                        + 'not rebuilt');
                    return;
                }

                show(step + 1);
            });
        }

        if (back) back.addEventListener('click', function () { show(step - 1); });

        // Whichever menu it was opened from is named in the dialog, because the same
        // flow is reachable from Guests, Seating and ticket management.
        all('[data-cap-from]').forEach(function (button) {
            button.addEventListener('click', function () {
                var source = flow.querySelector('[data-cap-source]');
                if (source) source.textContent = button.getAttribute('data-cap-from');

                countField.value = '0';
                zones.forEach(function (z) { z.added = 0; });
                manual = false;
                show(1);
            });
        });

        show(1);
    }

    /* ------------------------------------------------------------------------
       68. RSVP guest list
       --------------------------------------------------------------------------
       One list, however a guest got onto it. Manual adds happen in a single dialog
       that clears itself between guests — the previous design grew a new form per
       guest and turned the page into a scroll. Nothing here uses window.confirm or
       window.open: a blocked popup is a dead feature on the device it is blocked
       on, so deletes and imports are the same in-page modal as everywhere else.
       ---------------------------------------------------------------------- */

    function initGuestList() {
        var body = document.getElementById('guestlist-body');
        if (!body) return;

        var modal = document.getElementById('guestadd-modal');
        var editing = null;
        var pendingDelete = null;
        var seq = 100;

        function field(name) {
            return document.querySelector('[data-guest-field="' + name + '"]');
        }

        function rows() {
            return all('[data-guest-row]', body);
        }

        function recount() {
            var list = rows();
            var imported = list.filter(function (row) {
                return row.getAttribute('data-guest-source') === 'Imported';
            }).length;

            var count = document.querySelector('[data-guestlist-count]');
            var breakdown = document.querySelector('[data-guestlist-breakdown]');
            var empty = document.querySelector('[data-guestlist-empty]');

            if (count) {
                count.textContent = list.length + ' ' + (list.length === 1 ? 'guest' : 'guests');
            }
            if (breakdown) {
                breakdown.textContent = imported + ' imported, ' + (list.length - imported)
                    + ' added by hand';
            }
            if (empty) empty.hidden = list.length !== 0;

            // An invitation with nobody to send it to is a mistake, not a half-action.
            all('[data-rsvp-gated]').forEach(function (button) {
                button.disabled = list.length === 0;
                button.title = list.length ? '' : 'Add at least one guest first';
            });

            countGroups();
        }

        function clearForm() {
            ['name', 'email', 'phone'].forEach(function (name) {
                var el = field(name);
                if (el) el.value = '';
            });
            var tier = field('tier');
            if (tier) tier.selectedIndex = 0;

            var error = document.querySelector('[data-guest-error]');
            if (error) error.hidden = true;
        }

        function heading(text, save, again) {
            var h = document.querySelector('[data-guest-heading]');
            var s2 = document.querySelector('[data-guest-savelabel]');
            var a = document.querySelector('[data-guest-againlabel]');
            if (h) h.textContent = text;
            if (s2) s2.textContent = save;
            if (a) a.parentNode.hidden = !again;
        }

        function build(name, email, phone, tier) {
            var key = 'g' + (seq += 1);
            var initials = name.split(/\s+/).slice(0, 2).map(function (w) {
                return w.charAt(0).toUpperCase();
            }).join('');

            var tr = document.createElement('tr');
            tr.setAttribute('data-guest-row', '');
            tr.setAttribute('data-guest', key);
            tr.setAttribute('data-guest-state', 'unsent');
            tr.setAttribute('data-guest-tier', tier);
            tr.setAttribute('data-guest-source', 'Added by hand');
            tr.setAttribute('data-repeat-row', '');
            tr.className = 'is-new';

            tr.innerHTML =
                '<td><div class="cellstack"><span class="avatar" aria-hidden="true">' + initials
                + '</span><span class="cellstack__body">'
                + '<span class="cellstack__name" data-guest-name>' + name + '</span>'
                + '<span class="cellsub"><span data-guest-email>' + (email || '—')
                + '</span> · <span data-guest-phone>' + (phone || '—') + '</span></span>'
                + '</span></div></td>'
                + '<td><span class="badge badge--nodot badge--role" data-guest-tierlabel>'
                + tier + '</span></td>'
                + '<td class="u-muted">Added by hand</td>'
                + '<td><span class="badge badge--draft" data-guest-statelabel>Not yet sent</span></td>'
                + '<td><div class="rowactions">'
                + '<button class="iconbtn" type="button" data-guest-edit="' + key
                + '" data-modal-open="guestadd-modal">'
                + '<span class="u-visually-hidden">Edit ' + name + '</span>'
                + ICON_PENCIL + '</button>'
                + '<button class="iconbtn" type="button" data-guest-delete="' + key
                + '" data-modal-open="guestdel-modal">'
                + '<span class="u-visually-hidden">Delete ' + name + '</span>'
                + ICON_TRASH + '</button>'
                + '</div></td>';

            body.appendChild(tr);
            window.setTimeout(function () { tr.classList.remove('is-new'); }, 1400);
            return tr;
        }

        function readForm() {
            return {
                name: (field('name').value || '').trim(),
                email: (field('email').value || '').trim(),
                phone: (field('phone').value || '').trim(),
                tier: field('tier').value
            };
        }

        function valid(data) {
            var error = document.querySelector('[data-guest-error]');
            var ok = data.name && (data.email || data.phone);
            if (error) error.hidden = !!ok;
            if (!ok) field('name').focus();
            return ok;
        }

        all('[data-guest-save]').forEach(function (button) {
            button.addEventListener('click', function (event) {
                var data = readForm();
                if (!valid(data)) {
                    event.stopPropagation();
                    return;
                }

                if (editing) {
                    editing.querySelector('[data-guest-name]').textContent = data.name;
                    editing.querySelector('[data-guest-email]').textContent = data.email || '—';
                    editing.querySelector('[data-guest-phone]').textContent = data.phone || '—';
                    editing.querySelector('[data-guest-tierlabel]').textContent = data.tier;
                    editing.setAttribute('data-guest-tier', data.tier);
                    toast('<b>' + data.name + '</b> updated');
                    editing = null;
                } else {
                    build(data.name, data.email, data.phone, data.tier);
                    toast('<b>' + data.name + '</b> added to the guest list');
                }

                recount();

                // "Save & add another" keeps the dialog open with an empty form, which is
                // the whole point of not stamping a new form onto the page per guest.
                if (button.getAttribute('data-guest-save') === 'again') {
                    event.stopPropagation();
                    clearForm();

                    var flash = document.querySelector('[data-guest-added]');
                    var who = document.querySelector('[data-guest-addedname]');
                    if (who) who.textContent = data.name;
                    if (flash) {
                        flash.hidden = false;
                        window.setTimeout(function () { flash.hidden = true; }, 2600);
                    }

                    field('name').focus();
                } else {
                    clearForm();

                    // Closed by hand rather than with data-modal-close, so a form that
                    // fails validation above can keep the dialog open instead of losing
                    // what the host typed.
                    var closer = modal.querySelector('[data-modal-close]');
                    if (closer) closer.click();
                }
            });
        });

        all('[data-guest-new]').forEach(function (button) {
            button.addEventListener('click', function () {
                editing = null;
                clearForm();
                heading('Add a guest', 'Save guest', true);
                var flash = document.querySelector('[data-guest-added]');
                if (flash) flash.hidden = true;
            });
        });

        // Edit and delete are delegated, because rows are added after load.
        document.addEventListener('click', function (event) {
            var edit = event.target.closest && event.target.closest('[data-guest-edit]');
            if (edit) {
                var row = edit.closest('[data-guest-row]');
                editing = row;

                field('name').value = row.querySelector('[data-guest-name]').textContent.trim();
                var mail = row.querySelector('[data-guest-email]').textContent.trim();
                var tel = row.querySelector('[data-guest-phone]').textContent.trim();
                field('email').value = mail === '—' ? '' : mail;
                field('phone').value = tel === '—' ? '' : tel;
                field('tier').value = row.getAttribute('data-guest-tier');

                heading('Edit ' + field('name').value, 'Save changes', false);
                var flash = document.querySelector('[data-guest-added]');
                if (flash) flash.hidden = true;
                return;
            }

            var del = event.target.closest && event.target.closest('[data-guest-delete]');
            if (!del) return;

            pendingDelete = del.closest('[data-guest-row]');
            var name = pendingDelete.querySelector('[data-guest-name]').textContent.trim();
            var state = pendingDelete.querySelector('[data-guest-statelabel]').textContent.trim();

            var nameEl = document.querySelector('[data-del-name]');
            var metaEl = document.querySelector('[data-del-meta]');
            var noteEl = document.querySelector('[data-del-note]');
            if (nameEl) nameEl.textContent = name;
            if (metaEl) {
                metaEl.textContent = pendingDelete.querySelector('[data-guest-email]')
                    .textContent.trim() + ' · ' + state;
            }
            if (noteEl) {
                // Deleting somebody who already holds a live invitation is a different
                // act from deleting somebody who was never contacted.
                noteEl.textContent = state === 'Not yet sent'
                    ? 'They come off the list and will not be invited.'
                    : 'They come off the list and the invitation they hold stops working. '
                    + 'The credits already spent on it are not refunded.';
            }
        });

        var confirmDel = document.querySelector('[data-guest-delconfirm]');
        if (confirmDel) {
            confirmDel.addEventListener('click', function () {
                if (!pendingDelete) return;
                var name = pendingDelete.querySelector('[data-guest-name]').textContent.trim();
                pendingDelete.parentNode.removeChild(pendingDelete);
                pendingDelete = null;
                recount();
                toast('<b>' + name + '</b> removed from the guest list', 'warn');
            });
        }

        recount();
    }

    var ICON_PENCIL = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"'
        + ' stroke-width="1.7" aria-hidden="true"><path d="M4 20h4l10-10-4-4L4 16v4Z"/>'
        + '<path d="m14 6 4 4"/></svg>';

    var ICON_TRASH = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor"'
        + ' stroke-width="1.7" aria-hidden="true"><path d="M4 7h16"/>'
        + '<path d="M9 7V5h6v2"/><path d="M6 7l1 13h10l1-13"/></svg>';

    /* ------------------------------------------------------------------------
       69. Invitation recipients
       --------------------------------------------------------------------------
       "Send on" and "send time" answered a question nobody was asking — the real
       one is who gets this. Each group maps to a state on the list, so the counts
       are read off the list itself rather than stated. Groups are combined with
       OR, and a guest in two of them is still one message.
       ---------------------------------------------------------------------- */

    var GROUP_STATE = { 'new': 'unsent', expired: 'expired', pending: 'pending' };

    function countGroups() {
        var boxes = all('[data-group]');
        if (!boxes.length) return;

        var listRows = all('[data-guest-row]');
        var picked = all('[data-pick-guest]').filter(function (box) {
            return box.checked;
        }).map(function (box) { return box.getAttribute('data-pick-guest'); });

        var tierSelect = document.querySelector('[data-group-tier]');
        var chosen = {};
        var active = [];

        boxes.forEach(function (box) {
            var key = box.getAttribute('data-group');
            var extra = document.querySelector('[data-group-extra="' + key + '"]');
            if (extra) extra.hidden = !box.checked;

            var matched = listRows.filter(function (row) {
                if (GROUP_STATE[key]) {
                    return row.getAttribute('data-guest-state') === GROUP_STATE[key];
                }
                if (key === 'tier') {
                    return tierSelect && row.getAttribute('data-guest-tier') === tierSelect.value;
                }
                return picked.indexOf(row.getAttribute('data-guest')) !== -1;
            });

            var count = document.querySelector('[data-group-count="' + key + '"]');
            if (count) count.textContent = matched.length;

            if (!box.checked) return;
            active.push(box.parentNode.querySelector('.checkline__title').textContent.trim());

            // A guest in two groups is still one guest, so they are counted once.
            matched.forEach(function (row) { chosen[row.getAttribute('data-guest')] = true; });
        });

        var total = Object.keys(chosen).length;

        all('[data-group-total]').forEach(function (el) { el.textContent = total; });

        var pickedCount = document.querySelector('[data-picked-count]');
        if (pickedCount) pickedCount.textContent = picked.length;

        var note = document.querySelector('[data-group-note]');
        if (note) {
            note.textContent = !active.length ? 'Pick at least one group'
                : total === 0 ? 'Nobody on the list matches those groups yet'
                    : active.join(' + ');
        }

        var recap = document.querySelector('[data-group-recap]');
        if (recap) {
            recap.textContent = active.length ? active.join(' + ') : 'No group selected';
        }

        // The wallet charge follows the count, per channel.
        var emailOn = document.querySelector('[name="hub-channel-email"]');
        var waOn = document.querySelector('[name="hub-channel-whatsapp"]');
        var emailCost = (emailOn && emailOn.checked) ? total * 0.0015 : 0;
        var waCost = (waOn && waOn.checked) ? total * 35 : 0;

        // Email is priced per thousand, so a handful of guests costs a fraction of a
        // naira — printing ₦0 reads as broken, so a real but tiny charge says so.
        function money(el, value) {
            var node = document.querySelector(el);
            if (!node) return;
            node.textContent = (value > 0 && value < 1)
                ? 'under ₦1'
                : '₦' + Math.round(value).toLocaleString('en-US');
        }

        money('[data-send-emailcost]', emailCost);
        money('[data-send-wacost]', waCost);
        money('[data-send-total]', emailCost + waCost);

        all('[data-send-confirm]').forEach(function (button) {
            button.disabled = total === 0;
        });
    }

    function initGroups() {
        var boxes = all('[data-group]');
        if (!boxes.length) return;

        boxes.forEach(function (box) {
            box.addEventListener('change', countGroups);
        });

        var tier = document.querySelector('[data-group-tier]');
        if (tier) tier.addEventListener('change', countGroups);

        ['hub-channel-email', 'hub-channel-whatsapp'].forEach(function (name) {
            var box = document.querySelector('[name="' + name + '"]');
            if (box) box.addEventListener('change', countGroups);
        });

        all('[data-pick-guest]').forEach(function (box) {
            box.addEventListener('change', countGroups);
        });

        var save = document.querySelector('[data-picked-save]');
        if (save) {
            save.addEventListener('click', function () {
                countGroups();
                var n = all('[data-pick-guest]').filter(function (b) { return b.checked; }).length;
                toast(n + ' guest' + (n === 1 ? '' : 's') + ' picked by name');
            });
        }

        var send = document.querySelector('[data-send-confirm]');
        if (send) {
            send.addEventListener('click', function () {
                var total = document.querySelector('[data-group-total]').textContent;
                toast('<b>' + total + ' invitations</b> queued — replies land in the '
                    + 'responses table');
            });
        }

        countGroups();
    }

    /* ------------------------------------------------------------------------
       70. Invitation rich text
       --------------------------------------------------------------------------
       A host writing "you are invited" wants a heading, a bold date, a couple of
       bullets and a banner they can click. That is a small, closed set, so this is
       a small, closed editor built on the browser's own editing commands rather
       than a library — and the link and image dialogs are in-page modals, so the
       selection has to be remembered across opening one.
       ---------------------------------------------------------------------- */

    function initRichText() {
        var wrap = document.querySelector('[data-rte]');
        if (!wrap) return;

        var area = wrap.querySelector('[data-rte-area]');
        var saved = null;

        function remember() {
            var sel = window.getSelection();
            if (sel && sel.rangeCount && area.contains(sel.anchorNode)) {
                saved = sel.getRangeAt(0);
            }
        }

        function restore() {
            area.focus();
            if (!saved) return;
            var sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(saved);
        }

        function refresh() {
            all('[data-rte-cmd]', wrap).forEach(function (button) {
                var cmd = button.getAttribute('data-rte-cmd');
                if (cmd === 'removeFormat') return;
                try {
                    button.setAttribute('aria-pressed', String(document.queryCommandState(cmd)));
                } catch (err) {
                    button.setAttribute('aria-pressed', 'false');
                }
            });
        }

        area.addEventListener('keyup', function () { remember(); refresh(); });
        area.addEventListener('mouseup', function () { remember(); refresh(); });
        area.addEventListener('input', remember);
        area.addEventListener('blur', remember);

        all('[data-rte-cmd]', wrap).forEach(function (button) {
            // mousedown, not click — the selection is gone by the time click fires.
            button.addEventListener('mousedown', function (event) { event.preventDefault(); });
            button.addEventListener('click', function () {
                restore();
                document.execCommand(button.getAttribute('data-rte-cmd'), false, null);
                remember();
                refresh();
            });
        });

        all('[data-rte-block]', wrap).forEach(function (button) {
            button.addEventListener('mousedown', function (event) { event.preventDefault(); });
            button.addEventListener('click', function () {
                restore();
                document.execCommand('formatBlock', false, button.getAttribute('data-rte-block'));
                remember();
            });
        });

        // Opening a dialog moves focus out of the editor, so the range is captured
        // on the way out and put back when the insert lands.
        ['rtelink-modal', 'rteimg-modal'].forEach(function (id) {
            all('[data-modal-open="' + id + '"]').forEach(function (button) {
                button.addEventListener('mousedown', remember);
            });
        });

        function insert(html) {
            restore();
            document.execCommand('insertHTML', false, html);
            remember();
        }

        function clean(url) {
            // Only http(s) and mailto ever reach the guest's inbox from here.
            if (!url) return '';
            return /^(https?:|mailto:)/i.test(url) ? url : 'https://' + url.replace(/^\/+/, '');
        }

        var addLink = document.querySelector('[data-rte-linkadd]');
        if (addLink) {
            addLink.addEventListener('click', function () {
                var text = document.querySelector('[data-rte-linktext]');
                var url = document.querySelector('[data-rte-linkurl]');
                var href = clean(url.value.trim());
                if (!href) return;

                insert('<a href="' + href + '">' + (text.value.trim() || href) + '</a>&nbsp;');
                text.value = '';
                url.value = '';
                toast('Link added to the invitation');
            });
        }

        var addImg = document.querySelector('[data-rte-imgadd]');
        if (addImg) {
            addImg.addEventListener('click', function () {
                var file = document.querySelector('[data-rte-imgfile]');
                var alt = document.querySelector('[data-rte-imgalt]');
                var link = document.querySelector('[data-rte-imglink]');
                var src = file && file.files && file.files[0]
                    ? URL.createObjectURL(file.files[0])
                    : '';

                if (!src) {
                    toast('Choose an image first', 'warn');
                    return;
                }

                var img = '<img src="' + src + '" alt="' + (alt.value.trim() || '') + '">';
                var href = clean(link.value.trim());
                insert('<p>' + (href ? '<a href="' + href + '">' + img + '</a>' : img) + '</p>');

                alt.value = '';
                link.value = '';
                if (file) file.value = '';
                toast(href ? 'Clickable image added' : 'Image added to the invitation');
            });
        }

        // The preview shows exactly what the editor holds — same markup, guest frame.
        all('[data-modal-open="rtepreview-modal"]').forEach(function (button) {
            button.addEventListener('click', function () {
                var out = document.querySelector('[data-rte-preview]');
                if (out) out.innerHTML = area.innerHTML;
            });
        });

        refresh();
    }

    /* ------------------------------------------------------------------------
       71. Revoke an invitation
       --------------------------------------------------------------------------
       Revoking kills the link the guest already holds. What it cannot do is unspend
       the credits: the message was sent and delivered, so the charge stands. Saying
       so before the host confirms is the difference between a feature and a
       surprise on the next invoice.
       ---------------------------------------------------------------------- */

    function initRevoke() {
        var confirmBtn = document.querySelector('[data-revoke-confirm]');
        if (!confirmBtn) return;

        var target = null;

        document.addEventListener('click', function (event) {
            var button = event.target.closest && event.target.closest('[data-revoke]');
            if (!button) return;

            target = button.closest('[data-resp-row]');
            var name = target.querySelector('.cellstack__name').textContent.trim();
            var state = target.querySelector('[data-guest-statelabel]').textContent.trim();
            var ref = target.querySelector('[data-guest-ref]');

            var nameEl = document.querySelector('[data-revoke-name]');
            var metaEl = document.querySelector('[data-revoke-meta]');
            if (nameEl) nameEl.textContent = name;
            if (metaEl) {
                metaEl.textContent = state
                    + (ref && ref.textContent.trim() !== '—'
                        ? ' · ticket ' + ref.textContent.trim() : '');
            }
        });

        confirmBtn.addEventListener('click', function () {
            if (!target) return;

            var name = target.querySelector('.cellstack__name').textContent.trim();
            var badge = target.querySelector('[data-guest-statelabel]');

            target.setAttribute('data-guest-state', 'revoked');
            badge.textContent = 'Revoked';
            badge.className = 'badge badge--out';
            target.classList.add('is-revoked');

            // The link is dead, so every action that depended on it goes with it —
            // what is left is the one action that makes sense next.
            var actions = target.querySelector('.rowactions');
            if (actions) {
                actions.innerHTML = '<button class="btn btn--quiet btn--sm" type="button" '
                    + 'data-action="New invitation issued to ' + name
                    + ' — a fresh link, charged again">Re-issue</button>';
            }

            var mirror = document.querySelector('[data-guest-row][data-guest="'
                + target.getAttribute('data-guest') + '"] [data-guest-statelabel]');
            if (mirror) {
                mirror.textContent = 'Revoked';
                mirror.className = 'badge badge--out';
            }

            countGroups();
            toast('Invitation to <b>' + name + '</b> revoked — the link is dead, '
                + 'the credits stay spent', 'warn');
            target = null;
        });
    }

    /* ------------------------------------------------------------------------
       72. Seat assignment before ticket delivery
       --------------------------------------------------------------------------
       On a seated event a ticket without a seat is not a ticket, so Send stays
       disabled until every guest waiting on one has been placed. A guest can only
       be seated in their own tier's block: the alternative is a VIP holding a seat
       in the Early Bird row and finding out at the door.
       ---------------------------------------------------------------------- */

    function initSeatAssign() {
        var wrap = document.querySelector('[data-seatassign]');
        if (!wrap) return;

        var guests = all('[data-assign-guest]', wrap);
        var seats = all('[data-assign-seat]', wrap);
        var send = wrap.querySelector('[data-assign-send]');
        var label = wrap.querySelector('[data-assign-sendlabel]');
        var hint = wrap.querySelector('[data-assign-tierhint]');
        var current = null;
        var placed = {};

        function seated() {
            return Object.keys(placed).length;
        }

        function render() {
            var need = guests.length;
            var done = seated();

            var doneEl = wrap.querySelector('[data-assign-done]');
            var needEl = wrap.querySelector('[data-assign-need]');
            if (doneEl) doneEl.textContent = done;
            if (needEl) needEl.textContent = need;

            guests.forEach(function (guest) {
                var key = guest.getAttribute('data-assign-guest');
                var slot = guest.querySelector('[data-assign-seatof="' + key + '"]');
                var has = placed[key];

                guest.classList.toggle('is-seated', !!has);
                guest.setAttribute('aria-pressed', String(guest === current));
                if (slot) {
                    slot.textContent = has ? 'Seat ' + has : 'No seat yet';
                    slot.classList.toggle('assignguest__seat--set', !!has);
                }
            });

            var taken = {};
            Object.keys(placed).forEach(function (key) { taken[placed[key]] = key; });

            seats.forEach(function (seat) {
                var id = seat.getAttribute('data-assign-seat');
                var mine = taken[id];
                seat.classList.toggle('seatbtn--checked', !!mine);
                seat.setAttribute('aria-pressed', String(!!mine));

                // Out of tier is not an error to explain after the fact — it is simply
                // not offered.
                var offTier = current
                    && seat.getAttribute('data-assign-tier')
                    !== current.getAttribute('data-assign-guesttier');
                seat.classList.toggle('is-offtier', !!offTier && !mine);
            });

            if (hint) {
                hint.textContent = current
                    ? 'Placing ' + current.querySelector('.assignguest__name').textContent.trim()
                    + ' — ' + current.getAttribute('data-assign-guesttier') + ' seats only'
                    : 'Pick a guest first';
            }

            if (send) send.disabled = need === 0 || done !== need;
            if (label) {
                label.textContent = need === 0 ? 'Nothing to send'
                    : done === need ? 'Send ' + need + ' ticket' + (need === 1 ? '' : 's')
                        : (need - done) + ' still to seat';
            }
        }

        guests.forEach(function (guest) {
            guest.addEventListener('click', function () {
                current = current === guest ? null : guest;
                render();
            });
        });

        seats.forEach(function (seat) {
            seat.addEventListener('click', function () {
                if (seat.disabled) return;

                var id = seat.getAttribute('data-assign-seat');

                // Clicking an assigned seat frees it, which is how a mistake is undone.
                var holder = Object.keys(placed).filter(function (key) {
                    return placed[key] === id;
                })[0];
                if (holder) {
                    delete placed[holder];
                    render();
                    return;
                }

                if (!current) {
                    toast('Pick the guest first, then their seat', 'warn');
                    return;
                }
                if (seat.getAttribute('data-assign-tier')
                    !== current.getAttribute('data-assign-guesttier')) {
                    toast('That seat is in a different tier', 'warn');
                    return;
                }

                placed[current.getAttribute('data-assign-guest')] = id;

                // Move on to whoever is still standing, so a run of guests is one click each.
                var next = guests.filter(function (g) {
                    return !placed[g.getAttribute('data-assign-guest')];
                })[0];
                current = next || null;
                render();
            });
        });

        if (send) {
            send.addEventListener('click', function () {
                toast('<b>' + seated() + ' tickets</b> sent — each one carries its seat');
            });
        }

        render();
    }

    /* ------------------------------------------------------------------------
       73. Check-in guest list — filters, search and the seating layout
       --------------------------------------------------------------------------
       A door is worked one session at a time, so the list is filtered by date,
       slot and tier rather than shown whole. The slot options are derived from the
       rows themselves, so a date with two slots offers two — no separate table to
       keep in step with the data.
  
       The search is the other half: a guest at the door is identified by whatever
       they can produce, which might be a reference, a name or the email the ticket
       went to. One field takes all three, and the match can be checked in from the
       result without going back to the table.
       ---------------------------------------------------------------------- */

    function initCheckinList() {
        var table = document.getElementById('manual-table');
        if (!table) return;

        var rows = all('[data-guest-row]', table);
        var find = document.querySelector('[data-ci-find]');
        var results = document.querySelector('[data-ci-results]');
        var none = document.querySelector('[data-ci-none]');

        // day -> [[slot key, slot label], …], read off the rows so it cannot drift.
        var slotsByDay = {};
        var dayNames = {};

        rows.forEach(function (row) {
            var day = row.getAttribute('data-ci-day');
            var slot = row.getAttribute('data-ci-slot');
            var cells = row.cells;
            dayNames[day] = cells[3].textContent.trim();
            slotsByDay[day] = slotsByDay[day] || {};
            slotsByDay[day][slot] = cells[4].textContent.trim();
        });

        function fillSlots(daySelect, slotSelect) {
            var day = daySelect.value;
            var keep = slotSelect.value;
            slotSelect.innerHTML = '<option value="">All time slots</option>';

            var pool = day ? [day] : Object.keys(slotsByDay);
            var seen = {};

            pool.forEach(function (d) {
                Object.keys(slotsByDay[d] || {}).forEach(function (key) {
                    var label = slotsByDay[d][key];
                    if (seen[key + label]) return;
                    seen[key + label] = true;

                    var option = document.createElement('option');
                    option.value = key;
                    option.textContent = label;
                    slotSelect.appendChild(option);
                });
            });

            // Keep the host's slot if the new date still has it.
            slotSelect.value = keep;
            if (slotSelect.selectedIndex === -1) slotSelect.value = '';
        }

        /* ---- the filtered table ---- */
        var daySel = document.querySelector('[data-ci-filter="day"]');
        var slotSel = document.querySelector('[data-ci-filter="slot"]');
        var tierSel = document.querySelector('[data-ci-filter="tier"]');

        function apply() {
            var day = daySel.value;
            var slot = slotSel.value;
            var tier = tierSel.value;
            var shown = 0;
            var inCount = 0;

            rows.forEach(function (row) {
                var ok = (!day || row.getAttribute('data-ci-day') === day)
                    && (!slot || row.getAttribute('data-ci-slot') === slot)
                    && (!tier || row.getAttribute('data-ci-tier') === tier);

                row.hidden = !ok;
                if (!ok) return;

                shown += 1;
                if (row.cells[6].textContent.trim() === 'Checked-in') inCount += 1;
            });

            var count = document.querySelector('[data-ci-count]');
            var inEl = document.querySelector('[data-ci-in]');
            var outEl = document.querySelector('[data-ci-out]');
            var empty = document.querySelector('[data-ci-empty]');
            var scope = document.querySelector('[data-ci-scopelabel]');

            if (count) {
                count.textContent = shown + ' ' + (shown === 1 ? 'guest' : 'guests');
            }
            if (inEl) inEl.textContent = inCount;
            if (outEl) outEl.textContent = shown - inCount;
            if (empty) empty.hidden = shown !== 0;
            if (scope) {
                scope.textContent = [
                    day ? dayNames[day] : 'all dates',
                    slot ? (slotsByDay[day || Object.keys(slotsByDay)[0]][slot] || 'one slot')
                        : 'all slots',
                    tier ? tier : 'all tiers'
                ].join(', ');
            }
        }

        [daySel, slotSel, tierSel].forEach(function (select) {
            if (!select) return;
            select.addEventListener('change', function () {
                if (select === daySel) fillSlots(daySel, slotSel);
                apply();
            });
        });

        var clear = document.querySelector('[data-ci-clear]');
        if (clear) {
            clear.addEventListener('click', function () {
                daySel.value = '';
                tierSel.value = '';
                fillSlots(daySel, slotSel);
                slotSel.value = '';
                apply();
                toast('Filters cleared — showing every guest');
            });
        }

        fillSlots(daySel, slotSel);
        apply();

        /* ---- one field, three ways to be identified ---- */
        function card(row) {
            var cells = row.cells;
            var name = cells[1].querySelector('.cellstack__name').textContent.trim();
            var email = cells[1].querySelector('.cellsub').textContent.trim();
            var checked = cells[6].textContent.trim() === 'Checked-in';

            return '<article class="findcard' + (checked ? ' is-in' : '') + '">'
                + '<span class="findcard__ref">' + cells[0].textContent.trim() + '</span>'
                + '<span class="findcard__body">'
                + '<span class="findcard__name">' + name + '</span>'
                + '<span class="findcard__meta">' + email + '</span>'
                + '<span class="findcard__meta">' + cells[2].textContent.trim() + ' · '
                + cells[3].textContent.trim() + ' · ' + cells[4].textContent.trim()
                + ' · Seat ' + cells[5].textContent.trim() + '</span>'
                + '</span>'
                + '<span class="findcard__act">'
                + (checked
                    ? '<span class="badge badge--nodot badge--live">Already checked in</span>'
                    : '<button class="btn btn--primary btn--sm" type="button" data-ci-let="'
                    + row.getAttribute('data-ci-ref') + '">Check in &amp; allow</button>')
                + '</span></article>';
        }

        function search() {
            var term = find.value.trim().toLowerCase();

            if (term.length < 2) {
                results.hidden = true;
                results.innerHTML = '';
                if (none) none.hidden = true;
                return;
            }

            var hits = rows.filter(function (row) {
                return row.textContent.toLowerCase().indexOf(term) !== -1;
            }).slice(0, 6);

            results.innerHTML = hits.map(card).join('');
            results.hidden = hits.length === 0;
            if (none) none.hidden = hits.length !== 0;
        }

        if (find) find.addEventListener('input', search);

        // Checking in from a search result updates the row it came from, so the two
        // views cannot disagree about who is inside.
        document.addEventListener('click', function (event) {
            var button = event.target.closest && event.target.closest('[data-ci-let]');
            if (!button) return;

            var ref = button.getAttribute('data-ci-let');
            var row = rows.filter(function (r) {
                return r.getAttribute('data-ci-ref') === ref;
            })[0];
            if (!row) return;

            var name = row.cells[1].querySelector('.cellstack__name').textContent.trim();
            var badge = row.cells[6].querySelector('.badge');
            badge.textContent = 'Checked-in';
            badge.className = 'badge badge--live';
            row.cells[7].innerHTML =
                '<div class="rowactions"><span class="badge badge--nodot badge--live">'
                + 'Checked in</span></div>';

            bumpCounters();
            apply();
            search();
            markSeatIn(row.cells[5].textContent.trim());
            toast('<b>' + name + '</b> checked in — seat '
                + row.cells[5].textContent.trim());
        });

        function bumpCounters() {
            var into = document.querySelector('[data-count="checkedin"]');
            var left = document.querySelector('[data-count="remaining"]');
            if (into) into.textContent = (parseInt(into.textContent.replace(/\D/g, ''), 10) + 1)
                .toLocaleString('en-US');
            if (left) {
                left.textContent = Math.max(0,
                    parseInt(left.textContent.replace(/\D/g, ''), 10) - 1).toLocaleString('en-US');
            }
        }
    }

    // Shared so a check-in from any view lights the seat up.
    function markSeatIn(seatId) {
        var seat = document.querySelector('[data-ciseat="' + seatId + '"]');
        if (!seat) return;

        seat.classList.remove('ciseat--held');
        seat.classList.add('ciseat--in');
        seat.setAttribute('data-ci-state', 'in');

        var who = seat.querySelector('.ciseat__who');
        if (who) {
            who.textContent = who.textContent.split('·')[0].trim() + ' · Checked in';
        }

        if (window.__ciSeatSync) window.__ciSeatSync();
    }

    function initSeatLayout() {
        var blocks = all('[data-ci-tierblock]');
        if (!blocks.length) return;

        var seats = all('[data-ciseat]');
        var daySel = document.querySelector('[data-cis-filter="day"]');
        var slotSel = document.querySelector('[data-cis-filter="slot"]');
        var tierSel = document.querySelector('[data-cis-filter="tier"]');

        var slotsByDay = {};
        var dayNames = {};

        all('[data-guest-row]').forEach(function (row) {
            var day = row.getAttribute('data-ci-day');
            dayNames[day] = row.cells[3].textContent.trim();
            slotsByDay[day] = slotsByDay[day] || {};
            slotsByDay[day][row.getAttribute('data-ci-slot')] = row.cells[4].textContent.trim();
        });

        function fillSlots() {
            var day = daySel.value;
            var keep = slotSel.value;
            slotSel.innerHTML = '<option value="">All time slots</option>';

            var pool = day ? [day] : Object.keys(slotsByDay);
            var seen = {};

            pool.forEach(function (d) {
                Object.keys(slotsByDay[d] || {}).forEach(function (key) {
                    if (seen[key]) return;
                    seen[key] = true;
                    var option = document.createElement('option');
                    option.value = key;
                    option.textContent = slotsByDay[d][key];
                    slotSel.appendChild(option);
                });
            });

            slotSel.value = keep;
            if (slotSel.selectedIndex === -1) slotSel.value = '';
        }

        function apply() {
            var day = daySel.value;
            var slot = slotSel.value;
            var tier = tierSel.value;
            var occupied = 0;
            var held = 0;
            var free = 0;

            blocks.forEach(function (block) {
                block.hidden = !!tier && block.getAttribute('data-ci-tierblock') !== tier;
            });

            seats.forEach(function (seat) {
                var state = seat.getAttribute('data-ci-state');
                var inTier = !tier || seat.getAttribute('data-ci-tier') === tier;

                // A free seat has no date or slot of its own, so it belongs to every
                // filter — it is free whenever you look at it.
                var inScope = state === 'free'
                    || ((!day || seat.getAttribute('data-ci-day') === day)
                        && (!slot || seat.getAttribute('data-ci-slot') === slot));

                var show = inTier && inScope;
                seat.classList.toggle('is-dim', !show);

                if (!show) return;
                if (state === 'in') occupied += 1;
                else if (state === 'held') held += 1;
                else free += 1;
            });

            var o = document.querySelector('[data-cis-occupied]');
            var h = document.querySelector('[data-cis-held]');
            var f = document.querySelector('[data-cis-free]');
            var scope = document.querySelector('[data-cis-scopelabel]');

            if (o) o.textContent = occupied;
            if (h) h.textContent = held;
            if (f) f.textContent = free;
            if (scope) {
                scope.textContent = [
                    day ? dayNames[day] : 'All dates',
                    slot ? (slotsByDay[day || Object.keys(slotsByDay)[0]][slot] || 'one slot')
                        : 'all slots',
                    tier ? tier : 'all tiers'
                ].join(', ');
            }
        }

        [daySel, slotSel, tierSel].forEach(function (select) {
            if (!select) return;
            select.addEventListener('change', function () {
                if (select === daySel) fillSlots();
                apply();
            });
        });

        // Reading a seat should say who is in it without leaving the layout.
        seats.forEach(function (seat) {
            seat.addEventListener('click', function () {
                var guest = seat.getAttribute('data-ci-guest');
                var id = seat.getAttribute('data-ciseat');
                toast(guest
                    ? '<b>' + id + '</b> — ' + guest + ' · '
                    + (seat.getAttribute('data-ci-state') === 'in'
                        ? 'checked in' : 'assigned, not arrived')
                    : '<b>' + id + '</b> is free');
            });
        });

        window.__ciSeatSync = apply;
        fillSlots();
        apply();
    }

    /* ------------------------------------------------------------------------
       74. Payout account — country-specific fields and bank verification
       --------------------------------------------------------------------------
       The country decides both the form and the check that runs on it. Nigeria
       resolves a name from ten digits and a bank; South Africa cannot confirm
       anything without an account type and an identity document, so it asks for
       those first and the Verify button stays disabled until it has them.
  
       The account name is never typed. It comes back from the bank, which is the
       whole point of verifying — a hand-typed name is how a payout reaches the
       wrong person.
       ---------------------------------------------------------------------- */

    var PAYOUT_NAMES = {
        '0123456789': 'TECHSPHERE EVENTS LIMITED',
        '2244668800': 'KOFI ASANTE',
        '9988776655': 'TECHSPHERE MEDIA LIMITED',
        '1234567890': 'THABO M NKOSI',
        '6250554321': 'SIYANDA EVENTS PTY LTD'
    };

    function initPayoutAccount() {
        var pickers = all('[data-pv-countrypick]');
        if (!pickers.length) return;

        var verified = {};

        function form(key) {
            return document.querySelector('[data-pv-form="' + key + '"]');
        }

        function current(picker) {
            return picker.value;
        }

        function digits(key) {
            var field = document.querySelector('[data-pv-acct="' + key + '"]');
            return field ? field.value.replace(/\D/g, '') : '';
        }

        function bankOf(key) {
            var field = document.querySelector('[data-pv-bank="' + key + '"]');
            if (!field) return null;

            var list = document.getElementById(field.getAttribute('list') || '');
            if (!list) return null;

            return all('option', list).filter(function (option) {
                return option.value.toLowerCase() === field.value.trim().toLowerCase();
            })[0] || null;
        }

        // South Africa needs every extra part before the bank will answer.
        function partsReady(key) {
            var scope = form(key);
            if (!scope) return true;

            return all('[data-pv-part]', scope).every(function (field) {
                return !field.hasAttribute('required') || field.value.trim() !== '';
            });
        }

        function paint(key) {
            var scope = form(key);
            if (!scope) return;

            var field = document.querySelector('[data-pv-acct="' + key + '"]');
            var want = parseInt(field.getAttribute('data-pv-len'), 10) || 10;
            var no = digits(key);
            var bank = bankOf(key);

            if (field.value !== no) field.value = no;

            var code = scope.querySelector('[data-pv-code="' + key + '"]');
            if (code) {
                code.textContent = bank ? bank.getAttribute('data-bank-code') : '—';
            }

            var enough = no.length >= (key === 'za' ? want - 1 : want);
            var ready = enough && !!bank && partsReady(key);
            var run = scope.querySelector('[data-pv-run="' + key + '"]');
            var label = scope.querySelector('[data-pv-label]');
            var name = scope.querySelector('[data-pv-name]');
            var meta = scope.querySelector('[data-pv-meta]');
            var box = scope.querySelector('[data-pv-box="' + key + '"]');

            if (run) run.disabled = !ready;

            // Any edit invalidates a verification that was already done — it was a
            // check on the old number, not this one.
            if (verified[key] && verified[key].no !== no) delete verified[key];

            if (verified[key]) {
                if (box) box.classList.add('is-verified');
                if (label) label.textContent = 'Verified with the bank';
                if (name) name.textContent = verified[key].name;
                if (meta) {
                    meta.textContent = verified[key].bank + ' · code '
                        + verified[key].code + ' · ' + verified[key].no;
                }
                holder(key);
                return;
            }

            if (box) box.classList.remove('is-verified');
            if (label) label.textContent = 'Not verified yet';
            if (name) {
                name.textContent = !enough
                    ? (key === 'za' ? 'Enter the account number' : want + ' digits needed')
                    : !bank ? 'Pick the bank from the list'
                        : !partsReady(key)
                            ? 'Fill in the account type and identity document'
                            : 'Ready to verify';
            }
            if (meta) meta.textContent = '';
            holder(key);
        }

        function holder(key) {
            var got = verified[key];

            function put(sel, text) {
                var el = document.querySelector(sel);
                if (el) el.textContent = text;
            }

            put('[data-pv-holder]', got ? got.name : 'Not verified yet');
            put('[data-pv-holderbank]', got ? got.bank : '—');
            put('[data-pv-holderacct]', got ? got.no : '—');
            put('[data-pv-holdercode]', got ? got.code : '—');
            put('[data-pv-holderstate]', got ? 'Yes · ' + got.when : 'No');
        }

        /* ---- country switch ---- */
        pickers.forEach(function (picker) {
            // Each picker owns the forms that share its namespace, so the wizard's
            // copy and the hub's copy do not reach into each other.
            var ns = picker.id.replace(/pv-country$/, '');

            function swap() {
                var key = current(picker);

                all('[data-pv-form]').forEach(function (block) {
                    var owned = block.getAttribute('data-pv-form').indexOf(ns) === 0;
                    if (!owned) return;
                    block.hidden = block.getAttribute('data-pv-form') !== key;
                });

                paint(key);
            }

            picker.addEventListener('change', swap);
            swap();
        });

        /* ---- every input repaints its own country's block ---- */
        all('[data-pv-form]').forEach(function (scope) {
            var key = scope.getAttribute('data-pv-form');

            all('input, select', scope).forEach(function (field) {
                field.addEventListener('input', function () { paint(key); });
                field.addEventListener('change', function () { paint(key); });
            });

            var run = scope.querySelector('[data-pv-run="' + key + '"]');
            if (!run) return;

            run.addEventListener('click', function () {
                var bank = bankOf(key);
                var no = digits(key);
                var label = scope.querySelector('[data-pv-label]');
                var name = scope.querySelector('[data-pv-name]');
                var runLabel = scope.querySelector('[data-pv-runlabel]');

                if (label) label.textContent = 'Checking with the bank';
                if (name) name.textContent = 'Asking ' + bank.value + ' to confirm the name…';
                if (runLabel) runLabel.textContent = 'Verifying…';
                run.disabled = true;

                window.setTimeout(function () {
                    verified[key] = {
                        no: no,
                        bank: bank.value,
                        code: bank.getAttribute('data-bank-code'),
                        // An unknown number still resolves, because a real lookup returns
                        // whatever the bank holds rather than nothing.
                        name: PAYOUT_NAMES[no] || 'TECHSPHERE EVENTS LIMITED',
                        when: 'just now'
                    };

                    if (runLabel) runLabel.textContent = 'Verify again';
                    run.disabled = false;
                    paint(key);
                    toast('Account verified — <b>' + verified[key].name
                        + '</b> confirmed by ' + bank.value);
                }, 700);
            });
        });

        // "Later, from the Event Hub" needs its own note, since data-reveal only
        // handles the form block.
        all('[name="payout-when"]').forEach(function (radio) {
            radio.addEventListener('change', function () {
                var note = document.querySelector('[data-payout-later]');
                if (note) note.hidden = radio.value !== 'later' || !radio.checked;
            });
        });

        all('[data-pv-form]').forEach(function (scope) {
            paint(scope.getAttribute('data-pv-form'));
        });

        // Whichever country the select holds on load gets its form shown. The hub's
        // copy is namespaced ("hub-ng"), so the key is never assumed.
        pickers.forEach(function (picker) {
            var shown = form(current(picker));
            if (shown) shown.hidden = false;
        });
    }

    /* ------------------------------------------------------------------------
       75. Add-ons and promo codes in the hub
       --------------------------------------------------------------------------
       An add-on belongs to a slot, so the panel filters by slot — and an add-on
       scoped to the whole event stays visible under every filter, because it
       genuinely is on sale then. Pausing is reversible and shown as such; raising
       stock on a sold-out extra puts it straight back on sale.
       ---------------------------------------------------------------------- */

    function initHubExtras() {
        var cards = all('[data-addon-row]');
        if (!cards.length) return;

        var scope = document.querySelector('[data-addon-scope]');
        var picked = null;

        function recount() {
            var shown = cards.filter(function (card) { return !card.hidden; });
            var live = shown.filter(function (card) {
                return card.getAttribute('data-addon-state') === 'live';
            }).length;
            var out = shown.filter(function (card) {
                return card.getAttribute('data-addon-state') === 'out';
            }).length;

            function put(sel, text) {
                var el = document.querySelector(sel);
                if (el) el.textContent = text;
            }

            put('[data-addon-count]', shown.length + ' '
                + (shown.length === 1 ? 'extra' : 'extras'));
            put('[data-addon-live]', live);
            put('[data-addon-out]', out);

            var label = document.querySelector('[data-addon-scopelabel]');
            if (label && scope) {
                label.textContent = scope.value
                    ? scope.options[scope.selectedIndex].textContent.trim()
                    : 'every date and slot';
            }

            var empty = document.querySelector('[data-addon-empty]');
            if (empty) empty.hidden = shown.length !== 0;
        }

        function apply() {
            var want = scope ? scope.value : '';

            cards.forEach(function (card) {
                var own = card.getAttribute('data-addon-scope');
                // "all" is on sale on every slot, so it never filters out.
                card.hidden = !!want && own !== want && own !== 'all';
            });

            recount();
        }

        if (scope) scope.addEventListener('change', apply);

        /* ---- pause and resume, in place ---- */
        all('[data-addon-state]').forEach(function (button) {
            button.addEventListener('click', function () {
                var parts = button.getAttribute('data-addon-state').split('|');
                var card = document.querySelector('[data-addon="' + parts[0] + '"]');
                if (!card) return;

                var badge = card.querySelector('[data-addon-badge]');
                card.setAttribute('data-addon-state', parts[1]);
                badge.textContent = parts[1] === 'live' ? 'On sale' : 'Paused';
                badge.className = 'badge badge--' + (parts[1] === 'live' ? 'live' : 'paused');
                recount();
            });
        });

        /* ---- more stock puts a sold-out extra back on sale ---- */
        all('[data-addon-pick]').forEach(function (button) {
            button.addEventListener('click', function () {
                picked = document.querySelector('[data-addon="'
                    + button.getAttribute('data-addon-pick') + '"]');
                if (!picked) return;

                var name = picked.querySelector('[data-addon-name]').textContent.trim();
                var figures = picked.querySelector('.addoncard__figures')
                    .textContent.replace(/\s+/g, ' ').trim();

                function put(sel, text) {
                    var el = document.querySelector(sel);
                    if (el) el.textContent = text;
                }

                put('[data-stock-name]', name);
                put('[data-stock-meta]', figures);
                put('[data-addon-heading]', 'Edit ' + name);
                put('[data-addon-savelabel]', 'Save changes');

                var nameField = document.querySelector('[data-addon-field="name"]');
                if (nameField) nameField.value = name;
            });
        });

        all('[data-addon-new]').forEach(function (button) {
            button.addEventListener('click', function () {
                picked = null;

                function put(sel, text) {
                    var el = document.querySelector(sel);
                    if (el) el.textContent = text;
                }

                put('[data-addon-heading]', 'Add an optional extra');
                put('[data-addon-savelabel]', 'Add extra');

                all('[data-addon-field]').forEach(function (field) { field.value = ''; });
            });
        });

        var stockSave = document.querySelector('[data-stock-save]');
        if (stockSave) {
            stockSave.addEventListener('click', function () {
                if (!picked) return;

                var more = document.querySelector('[data-stock-more]');
                var added = parseInt((more && more.value || '0').replace(/\D/g, ''), 10) || 0;
                var name = picked.querySelector('[data-addon-name]').textContent.trim();
                var badge = picked.querySelector('[data-addon-badge]');

                picked.setAttribute('data-addon-state', 'live');
                badge.textContent = 'On sale';
                badge.className = 'badge badge--live';
                recount();

                toast('<b>' + added + ' more</b> ' + name
                    + ' — back on sale at the same price');
            });
        }

        var addonSave = document.querySelector('[data-addon-save]');
        if (addonSave) {
            addonSave.addEventListener('click', function () {
                var nameField = document.querySelector('[data-addon-field="name"]');
                var name = nameField ? nameField.value.trim() : '';
                toast(picked
                    ? '<b>' + (name || 'The extra') + '</b> updated'
                    : '<b>' + (name || 'New extra') + '</b> added — on sale now');
            });
        }

        /* ---- promo codes ---- */
        all('[data-code-state]').forEach(function (button) {
            button.addEventListener('click', function () {
                var parts = button.getAttribute('data-code-state').split('|');
                var row = document.querySelector('[data-code="' + parts[0] + '"]');
                if (!row) return;

                var badge = row.querySelector('[data-code-badge]');
                badge.textContent = parts[1] === 'live' ? 'Active' : 'Paused';
                badge.className = 'badge badge--' + (parts[1] === 'live' ? 'live' : 'paused');
            });
        });

        all('[data-code-pick]').forEach(function (button) {
            button.addEventListener('click', function () {
                var code = button.getAttribute('data-code-pick');
                var row = document.querySelector('[data-code="' + code + '"]');
                if (!row) return;

                function put(sel, text) {
                    var el = document.querySelector(sel);
                    if (el) el.textContent = text;
                }

                put('[data-limit-code]', code);
                put('[data-limit-meta]', row.cells[3].textContent.replace(/\s+/g, ' ').trim()
                    + ' used · ' + row.cells[1].textContent.trim());
            });
        });

        var limitSave = document.querySelector('[data-limit-save]');
        if (limitSave) {
            limitSave.addEventListener('click', function () {
                var code = document.querySelector('[data-limit-code]');
                var cap = document.querySelector('[data-limit-cap]');
                var row = code
                    ? document.querySelector('[data-code="' + code.textContent.trim() + '"]')
                    : null;

                if (row) {
                    var badge = row.querySelector('[data-code-badge]');
                    badge.textContent = 'Active';
                    badge.className = 'badge badge--live';
                }

                toast('Limit raised to <b>' + (cap ? cap.value : '—')
                    + ' uses</b> — the code works again');
            });
        }

        apply();
    }

    /* ------------------------------------------------------------------------
       76. Find-a-guest shortcut
       ------------------------------------------------------------------------ */
    function initFindFocus() {
        all('[data-ci-focus]').forEach(function (button) {
            button.addEventListener('click', function () {
                var field = document.querySelector('[data-ci-find]');
                if (!field) return;

                field.scrollIntoView({ behavior: 'smooth', block: 'center' });
                field.focus();
                toast('Type a ticket reference, a name or an email');
            });
        });
    }

    /* ------------------------------------------------------------------------
       77. What is included with a ticket
       --------------------------------------------------------------------------
       This started as a fixed catalogue of extras, which was the wrong shape: what
       comes with a ticket is the host's own promise, not a list we hand them. So
       each item can be renamed, re-described, removed or added, per ticket type,
       and the whole block switches off for a tier that includes nothing.
  
       Added rows start unticked. A row that appeared already promising something
       would put a claim in front of a buyer that nobody typed.
       ---------------------------------------------------------------------- */

    function initIncludes() {
        var groups = all('[data-incl-group]');
        if (!groups.length) return;

        function summarise(group) {
            var tid = group.getAttribute('data-incl-group');
            var out = document.querySelector('[data-includes="' + tid + '"] [data-includes-value]');
            if (!out) return;

            var on = all('[data-incl-row]', group).filter(function (row) {
                var box = row.querySelector('[data-incl-box]');
                return box && box.checked;
            }).map(function (row) {
                return row.querySelector('[data-incl-label]').textContent.trim();
            });

            var switched = group.querySelector('[data-incl-switch]');
            var live = !switched || switched.getAttribute('aria-checked') === 'true';

            out.textContent = !live ? 'Sessions only — nothing extra'
                : on.length ? on.join(' · ')
                    : 'Nothing extra ticked yet';
        }

        groups.forEach(function (group) {
            var tid = group.getAttribute('data-incl-group');
            var body = group.querySelector('[data-incl-body="' + tid + '"]');
            var off = group.querySelector('[data-incl-off="' + tid + '"]');
            var switched = group.querySelector('[data-incl-switch]');

            if (switched) {
                // initSwitches already flips aria-checked; this reacts to it.
                switched.addEventListener('click', function () {
                    window.setTimeout(function () {
                        var on = switched.getAttribute('aria-checked') === 'true';
                        if (body) body.hidden = !on;
                        if (off) off.hidden = on;
                        summarise(group);
                    }, 0);
                });
            }

            summarise(group);
        });

        // Delegated, because rows are cloned in after load.
        document.addEventListener('click', function (event) {
            var target = event.target.closest
                && event.target.closest('[data-incl-edit], [data-incl-save]');
            if (!target) return;

            var row = target.closest('[data-incl-row]');
            var editing = target.hasAttribute('data-incl-edit');

            row.classList.toggle('is-editing', editing);
            row.querySelector('[data-incl-edit]').hidden = editing;
            row.querySelector('[data-incl-save]').hidden = !editing;

            if (editing) {
                var first = row.querySelector('[data-incl-labelfield]');
                if (first) first.focus();
                return;
            }

            // Saving copies the fields back into the label the buyer would read.
            var label = row.querySelector('[data-incl-labelfield]');
            var note = row.querySelector('[data-incl-notefield]');

            row.querySelector('[data-incl-label]').textContent =
                label.value.trim() || 'Untitled item';
            row.querySelector('[data-incl-note]').textContent = note.value.trim();

            var group = row.closest('[data-incl-group]');
            if (group) summarise(group);
            toast('<b>' + (label.value.trim() || 'The item') + '</b> saved');
        });

        document.addEventListener('change', function (event) {
            if (!event.target.hasAttribute
                || !event.target.hasAttribute('data-incl-box')) return;

            var group = event.target.closest('[data-incl-group]');
            if (group) summarise(group);
        });

        // The repeater clones the last row, so a new item arrives carrying somebody
        // else's text. It is emptied, left unticked — nothing should be promised to a
        // buyer that nobody typed — and opened for editing straight away.
        all('[data-repeat-add]').forEach(function (button) {
            var id = button.getAttribute('data-repeat-add');
            if (!id || id.indexOf('incl-') !== 0) return;

            button.addEventListener('click', function () {
                var list = document.getElementById(id);
                if (!list) return;

                window.setTimeout(function () {
                    var row = list.lastElementChild;
                    if (!row) return;

                    var box = row.querySelector('[data-incl-box]');
                    var label = row.querySelector('[data-incl-labelfield]');
                    var note = row.querySelector('[data-incl-notefield]');

                    if (box) box.checked = false;
                    if (label) label.value = '';
                    if (note) note.value = '';
                    row.querySelector('[data-incl-label]').textContent = 'New item';
                    row.querySelector('[data-incl-note]').textContent =
                        'Say what the buyer gets';

                    row.classList.add('is-editing');
                    row.querySelector('[data-incl-edit]').hidden = true;
                    row.querySelector('[data-incl-save]').hidden = false;
                    if (label) label.focus();

                    var group = row.closest('[data-incl-group]');
                    if (group) summarise(group);
                }, 0);
            });
        });
    }

    /* ------------------------------------------------------------------------
       78. Line breaks become bullets
       --------------------------------------------------------------------------
       Hosts write a description as a list whether or not the field is one. Treating
       each line as a bullet matches what they meant, and the preview shows the
       buyer's view while it is typed. One line stays a sentence — a single bullet
       is noise.
       ---------------------------------------------------------------------- */

    function initDescBullets() {
        var sources = all('[data-desc-src]');
        if (!sources.length) return;

        sources.forEach(function (field) {
            var key = field.getAttribute('data-desc-src');
            var wrap = document.querySelector('[data-desc-prev="' + key + '"]');
            if (!wrap) return;

            var list = wrap.querySelector('[data-desc-list]');
            var line = wrap.querySelector('[data-desc-line]');

            function render() {
                var lines = field.value.split('\n').map(function (l) {
                    return l.trim();
                }).filter(function (l) { return l.length; });

                if (lines.length > 1) {
                    list.hidden = false;
                    line.hidden = true;
                    list.innerHTML = lines.map(function (l) {
                        return '<li>' + l.replace(/[<>]/g, '') + '</li>';
                    }).join('');
                } else {
                    list.hidden = true;
                    line.hidden = false;
                    line.textContent = lines[0] || 'Nothing written yet';
                }
            }

            field.addEventListener('input', render);
            render();
        });
    }

    /* ------------------------------------------------------------------------
       79. Reuse branding from a previous event
       --------------------------------------------------------------------------
       A host running four events a year should not pick the same plum four times.
       Picking a saved set shows what applying it would change — named field by
       named field — and only then applies it, because silently overwriting a logo
       somebody just uploaded is worse than one extra click. Applied values stay
       editable, and the source event is not touched.
       ---------------------------------------------------------------------- */

    function initBrandReuse() {
        var cards = all('[data-brandsaved]');
        if (!cards.length) return;

        var bar = document.querySelector('[data-brandapply-bar]');
        var picked = null;

        // Paint each card's own swatches from its own values.
        cards.forEach(function (card) {
            var primary = card.querySelector('[data-swatch="primary"]');
            var accent = card.querySelector('[data-swatch="accent"]');
            if (primary) primary.style.backgroundColor = card.getAttribute('data-brand-primary');
            if (accent) accent.style.backgroundColor = card.getAttribute('data-brand-accent');
        });

        function describe(card) {
            var bits = [
                'logo ' + card.getAttribute('data-brand-logoname'),
                card.getAttribute('data-brand-primary') + ' primary',
                card.getAttribute('data-brand-accent') + ' accent',
                card.getAttribute('data-brand-theme') + ' theme',
                'cover ' + card.getAttribute('data-brand-covername')
            ];
            return 'Will set ' + bits.join(', ') + '.';
        }

        cards.forEach(function (card) {
            card.addEventListener('click', function () {
                picked = picked === card ? null : card;

                cards.forEach(function (other) {
                    other.setAttribute('aria-pressed', String(other === picked));
                });

                if (bar) bar.hidden = !picked;
                if (!picked) return;

                var name = document.querySelector('[data-brandapply-name]');
                var what = document.querySelector('[data-brandapply-what]');
                if (name) name.textContent = picked.getAttribute('data-brand-title');
                if (what) what.textContent = describe(picked);
            });
        });

        var cancel = document.querySelector('[data-brandapply-cancel]');
        if (cancel) {
            cancel.addEventListener('click', function () {
                picked = null;
                cards.forEach(function (c) { c.setAttribute('aria-pressed', 'false'); });
                if (bar) bar.hidden = true;
            });
        }

        var go = document.querySelector('[data-brandapply-go]');
        if (!go) return;

        go.addEventListener('click', function () {
            if (!picked) return;

            var primary = picked.getAttribute('data-brand-primary');
            var accent = picked.getAttribute('data-brand-accent');
            var theme = picked.getAttribute('data-brand-theme');

            // Colour inputs need a real event, or the preview never hears about it.
            function setColour(kind, value) {
                all('[data-brand-colour="' + kind + '"]').forEach(function (input) {
                    input.value = value;
                    input.dispatchEvent(new Event('input', { bubbles: true }));
                });

                // Keep the preset chips honest about which one is now active.
                all('[data-color-target="' + kind + '"]').forEach(function (chip) {
                    chip.setAttribute('aria-pressed',
                        String(chip.getAttribute('data-color').toLowerCase() === value.toLowerCase()));
                });
            }

            setColour('primary', primary);
            setColour('accent', accent);

            var themeBtn = document.querySelector('[data-pick="' + theme + '"]');
            if (themeBtn) themeBtn.click();

            // The logo and cover are named rather than re-uploaded: this prototype has
            // no file to copy, and saying which asset was brought over is the honest
            // version of that.
            function name(sel, verb, value) {
                var el = document.querySelector(sel);
                if (el) el.textContent = value;
                var verbEl = document.querySelector(verb);
                if (verbEl) verbEl.textContent = 'Replace';
            }

            name('[data-brand-logo-name]', '[data-brand-logo-verb]',
                picked.getAttribute('data-brand-logoname'));
            name('[data-brand-cover-name]', '[data-brand-cover-verb]',
                picked.getAttribute('data-brand-covername'));

            all('[data-brand-logo-box], [data-brand-cover-box]').forEach(function (box) {
                box.classList.add('is-filled');
            });

            picked.setAttribute('aria-pressed', 'false');
            if (bar) bar.hidden = true;

            toast('<b>' + picked.getAttribute('data-brand-title')
                + '</b> applied — everything stays editable');
            picked = null;
        });
    }

    /* ------------------------------------------------------------------------
       80. Payout ledger — scoping the escrow view
       --------------------------------------------------------------------------
       The releases read event-wide by default, because that is the number the
       business runs on. Narrowing to a date answers the other question — "we took
       twelve million on the Saturday and four came through" — so a release scoped
       to the whole event stays visible under every date, since it genuinely covers
       that date too. The released and held figures recompute from whatever is on
       screen rather than being restated.
       ---------------------------------------------------------------------- */

    function initEscrow() {
        var scope = document.querySelector('[data-esc-scope]');
        if (!scope) return;

        var rows = all('[data-esc-row]');
        var slotRows = all('[data-escslot-row]');

        function amount(cell) {
            return parseInt(cell.textContent.replace(/[^0-9]/g, ''), 10) || 0;
        }

        function money(value) {
            return '₦' + value.toLocaleString('en-US');
        }

        function apply() {
            var want = scope.value;
            var shown = 0;
            var released = 0;
            var held = 0;

            rows.forEach(function (row) {
                var own = row.getAttribute('data-esc-scope');
                // A whole-event release covers every date, so it is never filtered out.
                var show = !want || own === want || own === 'all';

                row.hidden = !show;
                if (!show) return;

                shown += 1;
                var value = amount(row.cells[4]);
                var state = row.cells[6].textContent.trim();

                if (state === 'Released') released += value;
                else held += value;
            });

            function put(sel, text) {
                var el = document.querySelector(sel);
                if (el) el.textContent = text;
            }

            put('[data-esc-count]', shown + ' ' + (shown === 1 ? 'phase' : 'phases'));
            put('[data-esc-released]', money(released));
            put('[data-esc-held]', money(held));
            put('[data-esc-scopelabel]', want
                ? scope.options[scope.selectedIndex].textContent.trim()
                : 'whole event');

            // The per-slot table follows the same scope, totals included.
            var charged = 0;
            var slotReleased = 0;

            slotRows.forEach(function (row) {
                var show = !want || row.getAttribute('data-esc-scope') === want;
                row.hidden = !show;
                if (!show) return;

                charged += amount(row.cells[1]);
                slotReleased += amount(row.cells[2]);
            });

            put('[data-escslot-charged]', money(charged));
            put('[data-escslot-released]', money(slotReleased));
            put('[data-escslot-left]', money(charged - slotReleased));
        }

        scope.addEventListener('change', apply);
        apply();
    }

    function init() {
        initEventType();
        initWizardLoader();
        initSaleMode();
        initPasswordToggles();
        initPasswordStrength();
        initRedirect();
        initSidebar();
        initViewSwitch();
        initTabs();
        initSwitches();
        initRepeaters();
        initReveals();
        initChoiceCards();
        initSegments();
        initSeatModes();
        initAccordions();
        initPickGroups();
        initUploads();
        initModals();
        initFilters();
        initSettingsNav();
        initPublish();
        initTimezone();
        initInlineEdit();
        initSchedule();
        initSteppers();
        initScopes();
        initSeatPicker();
        initAvailability();
        initFormPreview();
        initAutoNames();
        initImagePreview();
        initBullets();
        initStepCount();
        initSessionFilter();
        initIncludes();
        initActions();
        initTierVisibility();
        initRsvpGate();
        initPresets();
        initBroadcastTemplates();
        initEditorTools();
        initColorPresets();
        initRsvpFlag();
        initRoles();
        initSalesToggle();
        initCheckin();
        initSpeakers();
        initSpeakerPick();
        initFormTemplates();
        initSysCheck();
        initSegmentBuilder();
        initBrandPreview();
        initRowMenus();
        initCancelScope();
        initCrew();
        initTableFilter();
        initOrderStages();
        initFoodItems();
        initRegGate();
        initRegistrations();
        initScanner();
        initCreditPacks();
        initDocPick();
        initSeatJump();
        initBankLookup();
        initBuyCredits();
        initAddCapacity();
        initGuestList();
        initGroups();
        initRichText();
        initRevoke();
        initSeatAssign();
        initCheckinList();
        initSeatLayout();
        initPayoutAccount();
        initHubExtras();
        initFindFocus();
        initIncludes();
        initDescBullets();
        initBrandReuse();
        initEscrow();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
}());
