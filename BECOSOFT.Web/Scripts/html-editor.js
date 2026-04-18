class HTMLEditorContainer {
    uniqueCounter = 0;
    htmlEditors = [];

    getEditor(containerSelector, options) {
        let editor;

        let container = $(containerSelector);
        let editorIdentifier = container.data("quill-identifier");
        if (editorIdentifier && editorIdentifier.length > 0) {
            editor = this.htmlEditors[editorIdentifier];
        }

        if (!editor) {
            let currentIdentifier = this.uniqueCounter;
            this.uniqueCounter++;
            let editorSelector = containerSelector + " div[data-quill='editor']";
            container.data("quill-identifier", currentIdentifier);
            editor = new HTMLEditor(editorSelector, options);
            this.htmlEditors[currentIdentifier] = editor;
        }

        return editor;
    }

    getEditors() {
        return this.htmlEditors;
    }
}

var Embed = Quill.import('blots/block/embed');
Quill.register({
    'formats/html': Embed
});

class HTMLEditor extends Quill {
    constructor(selector, options) {
        super(selector, options);
    }

    getHtml() {
        return this.root.innerHTML;
    }

    setHtml(html) {
        this.setText("");
        this.clipboard.dangerouslyPasteHTML(0, html);
    }

    getHtmlFromRange(range) {
        let selectedContent = this.getContents(range.index, range.length);
        let tempContainer = document.createElement("div");
        let tempQuill = new HTMLEditor(tempContainer);
        tempQuill.setContents(selectedContent);
        return tempQuill.getHtml();
    }

    attachTooltipHandler(format, options) {
        let tooltip = new CustomizableTooltip(format, this, options);
    }

    attachHandler(format, handlerFunction) {
        let toolbar = this.getModule("toolbar");
        let editor = this;
        let handler = () => handlerFunction(editor);
        toolbar.addHandler(format, handler);
    }
}

// SOURCE: https://github.com/loagit/Quill-Examples-and-FAQ/blob/master/Quill%20Project%20004%20-%20Customizable%20Tooltip/app.js
// #################################################################################
//                                                                UTILITY FUNCTIONS
// #################################################################################

// See: https://stackoverflow.com/a/5085587
function hasClass(element, className) {
    return (' ' + element.className + ' ').indexOf(' ' + className + ' ') > -1;
}

/** See: https://quilljs.com/docs/api/#getcontents */
function getContentDelta(quill) {
    return quill.getContents();
}

/** See: https://github.com/quilljs/quill/issues/2698 */
function getContentText(quill) {
    return quill.getContents().ops.reduce((text, op) => {
        if (typeof op.insert === 'string') {
            return text + op.insert;
        }
        // If it is not a string, the newline character is set, which has
        // a length of only 1 character.
        else {
            return text + '\n';
        }
    }, '');
}

function extractVideoUrl(url) {
    let match =
        url.match(/^(?:(https?):\/\/)?(?:(?:www|m)\.)?youtube\.com\/watch.*v=([a-zA-Z0-9_-]+)/,) ||
            url.match(/^(?:(https?):\/\/)?(?:(?:www|m)\.)?youtu\.be\/([a-zA-Z0-9_-]+)/);
  
    if (match) {
        return `${match[1] || 'https'}://www.youtube.com/embed/${match[2]}?showinfo=0`;
    }
  
    // eslint-disable-next-line no-cond-assign
    if ((match = url.match(/^(?:(https?):\/\/)?(?:www\.)?vimeo\.com\/(\d+)/))) {
        return `${match[1] || 'https'}://player.vimeo.com/video/${match[2]}/`;
    }
    
    return url;
}

/** Customizable tooltip, which follows the same design as Quill Editor's native 
  * tooltip. Below are all the configurations that can be made, as well as a 
  * description of what each one represents.
  * 
  * [OPTIONS]
  * format = The format to be worked by tooltip.
  * inputLabel = Left label given to value input.
  * inputLabelClass = Overrides CSS class for value input label styling.
  * inputPlaceholder = Value input placeholder text (<input placeholder="...">).
  * inputClass = Overrides CSS class for value input styling.
  * actionText = Text for action "button" to the right of value input.
  * actionClass = Overrides CSS class for action text (the "button").
  * containerClass = Replaces CSS class for element that contains all others.
  * hideOnTyping = If true, tooltip will be hidden when typing in Quill.
  * hideOnAction = If true, tooltip will be hidden by clicking action text.
  * clearAfterHide = If true, the value input has its text cleared after tooltip is hidden.
  * defaultValue = Defines a default value for value input. If set, clearAfterHide is always false.
  * onAction = Function called when action text is clicked by the user. Setting a value for this property makes the user responsible for the tooltip action.
  * onShow = Function called when tooltip is revealed.
  * onHide = Function called when tooltip is hidden.
  */
class CustomizableTooltip {
    constructor(format, quill, options) {
        // Is everything ok here?
        this.checkState(format, quill);
        this.checkOptions(options);

        // Everything seems ok here...
        this.quill = quill;
        this.format = format;

        // Lets build...
        this.buildInterface();

        var thisTooltip = this;
        this.quill.getModule("toolbar").addHandler(this.format, function () {
            thisTooltip.toolbarButtonPressed();
        });

        this.quill.container.appendChild(this.container);
        this.hide();
    }

    // This prevents a surprise from appearing while using the editor. If a 
    // problem exists, it will appear when the tooltip is being built.
    checkState(format, quill) {
        // Is Quill reference useful?
        if (quill === null || typeof quill !== 'object')
            throw 'Quill reference was not passed in argument, or is null.';

        // Was the format specified?
        if (!format || format.length <= 0)
            throw 'No format was specified.';

        // Is the format registered?
        if (!Quill.import('formats/' + format))
            throw 'No format "' + format + '" found. Please, be sure to pass a format that is registered within Quill.';
    }

    /** Checks whether properties have been set correctly, or need to be 
      * overwritten if not. */
    checkOptions(options) {
        if (!options || options == null) options = {};

        if (!options.inputLabel || options.inputLabel.length <= 0)
            options.inputLabel = 'Value';
        if (!options.inputLabelClass || options.inputLabelClass.length <= 0)
            options.inputLabelClass = 'rs-tooltip-label';
        if (!options.inputPlaceholer || options.inputPlaceholer.left <= 0)
            options.inputPlaceholer = 'Insert value here...';
        if (!options.inputClass || options.inputClass.length <= 0)
            options.inputClass = 'rs-tooltip-input';
        if (!options.actionText || options.actionText.length <= 0)
            options.actionText = 'Conclude';
        if (!options.actionClass || options.actionClass.length <= 0)
            options.actionClass = 'rs-tooltip-action';
        if (!options.containerClass || options.containerClass.length <= 0)
            options.containerClass = 'rs-tooltip-container';
        if (!options.hideOnTyping)
            options.hideOnTyping = false;
        if (options.clearAfterHide && options.defaultValue && options.defaultValue.length >= 0)
            options.clearAfterHide = false;
        if (!options.hideOnAction || options.hideOnAction == false)
            options.hideOnAction = true;

        this.op = options;
    }

    buildInterface() {
        this.buildContainer();
        this.buildInputLabel();
        this.buildInput();
        this.buildAction();

        // Adds built elements into this tooltip container.
        this.container.appendChild(this.inputLabel);
        this.container.appendChild(this.input);
        this.container.appendChild(this.action);
    }

    buildContainer() {
        var thisTooltip = this;
        var container = document.createElement('DIV');
        container.classList.add(this.op.containerClass);

        // Hide tooltip by clicking outside of it.
        document.body.addEventListener('click', event => {
            // Was it clicked off?
            if (!hasClass(event.target, thisTooltip.op.containerClass)) {
                // Prevents tooltip from ever appearing if its button is clicked.
                // The button can have several internal elements. You can even 
                // click on the SVG element without clicking the button (the 
                // button icon, or the elements representing the icon). We take
                // this into account with closest, and verify that in addition
                // to the button, some element of it has been clicked.
                if (hasClass(event.target, 'ql-' + thisTooltip.format) ||
                    event.target.closest('.ql-' + thisTooltip.format)) {
                    return;
                }

                thisTooltip.hide();
            }
        });

        // Prevents tooltip from being hidden if its content is clicked.
        container.addEventListener('click', event => {
            event.stopPropagation();
        });

        // Hide tooltip when typing text in the editor.
        if (this.op.hideOnTyping) {
            this.quill.on('text-change', function (delta, oldDelta, source) {
                if (source === 'user') {
                    thisTooltip.hide();
                }
            });
        }

        this.container = container;
    }

    buildInputLabel() {
        var label = document.createElement('SPAN');
        label.innerText = this.op.inputLabel;
        label.classList.add(this.op.inputLabelClass);

        this.inputLabel = label;
    }

    buildInput() {
        var thisTooltip = this;
        var input = document.createElement('INPUT');
        input.type = 'text';
        input.classList.add(this.op.inputClass);
        input.placeholder = this.op.inputPlaceholer;
        input.value = this.op.defaultValue && this.op.defaultValue.length > 0 ?
            this.op.defaultValue :
            '';

        // Has the user added an event of his own to this tooltip? If so, it 
        // will be called as a priority.
        if (this.op.onAction && typeof this.op.onAction === 'function') {
            input.addEventListener('keydown', e => {
                if (e.key === 'Enter') {
                    this.op.onAction(input.value, e);
                    if (thisTooltip.op.hideOnAction) thisTooltip.hide();
                }
            });
        }
        // Otherwise, the tooltip calls the default implementation. It is
        // understood that the user knows how this tooltip works, and has
        // configured it correctly.
        else {
            input.addEventListener('keydown', e => {
                if (e.key === 'Enter') {
                    thisTooltip.insertEmbed(input.value, e);
                    if (thisTooltip.op.hideOnAction) thisTooltip.hide();
                }
            });
        }

        this.input = input;
    }

    buildAction() {
        var thisTooltip = this;
        var linkAction = document.createElement('a');
        linkAction.innerText = this.op.actionText;
        linkAction.classList.add(this.op.actionClass);

        // Has the user added an event of his own to this tooltip? If so, it 
        // will be called as a priority.
        if (this.op.onAction && typeof this.op.onAction === 'function') {
            linkAction.addEventListener('click', e => {
                this.op.onAction(thisTooltip.input.value, e);
                if (thisTooltip.op.hideOnAction) thisTooltip.hide();
            });
        }
        // Otherwise, the tooltip calls the default implementation. It is
        // understood that the user knows how this tooltip works, and has
        // configured it correctly.
        else {
            linkAction.addEventListener('click', e => {
                thisTooltip.insertEmbed(thisTooltip.input.value, e);
                if (thisTooltip.op.hideOnAction) thisTooltip.hide();
            });
        }

        this.action = linkAction;
    }

    toolbarButtonPressed() {
        if (this.isVisible()) {
            this.hide();
            return;
        }

        this.show();
    }

    // Created for the convenience of client code. Not necessarily this needs to be utilized.
    setInputLabel(label) {
        if (!label || label.length <= 0) return;
        this.inputLabel.innerText = label;
    }

    // Created for the convenience of client code. Not necessarily this needs to be utilized.
    setActionLabel(label) {
        if (!label || label.length <= 0) return;
        this.action.innerText = label;
    }

    // Created for the convenience of client code. Not necessarily this needs to be utilized.
    setInputValue(value) {
        if (!value || value.length <= 0) return;
        this.input.value = value;
    }

    // Created for the convenience of client code. Not necessarily this needs to be utilized.
    setInputPlaceholder(placeholder) {
        if (!placeholder || placeholder.length <= 0) return;
        this.input.placeholder = placeholder;
    }

    // Created for the convenience of client code. Not necessarily this needs to be utilized.
    getInputValue() {
        return this.input.value;
    }

    show() {
        if (this.isVisible()) return;

        this.container.classList.remove('ql-hidden');
        this.updatePosition();

        if (this.op.onShow && typeof onShow === 'function') this.op.onShow();
    }

    hide() {
        if (!this.isVisible()) return;

        this.container.classList.add('ql-hidden');

        if (!this.op.defaultValue && this.op.clearAfterHide) this.input.value = '';
        if (this.op.onHide && typeof onHide === 'function') this.op.onHide();
    }

    isVisible() {
        return !hasClass(this.container, 'ql-hidden');
    }

    updatePosition() {
        // quill.getBounds return a rectangle based on editor caret position. 
        // This is where we can locate where the window will appear. The idea 
        // here is to leave the horizontal center of the tooltip aligned with 
        // the editor caret (range index).
        let range = this.quill.getSelection();
        const bounds = this.quill.getBounds(range.index);

        // Tooltip left edge X.
        let x = bounds.left;// - this.container.offsetWidth / 2;
        let editorContainer = this.quill.root;

        // See: http://javascript.info/coordinates
        let leftBorderLimit = editorContainer.getBoundingClientRect().left;
        let rightBorderLimit = editorContainer.getBoundingClientRect().right;

        // Corrects left edge.
        if ((x * -1) > leftBorderLimit) {
            x = x + ((x * -1) - leftBorderLimit);
        }
        
        // Tooltip right edge X.
        let widthX = x + this.container.offsetWidth + 2;

        // Corrects right edge.
        if (widthX > rightBorderLimit) {
            x = x - (widthX - rightBorderLimit);
        }

        this.container.style.top =
            bounds.top +        // Y from upper left edge.
            bounds.height +     // Caret Height (one line).
            10 +                // One more small space to not get too on top.
            'px';
        this.container.style.left = x + 'px';
    }

    insertEmbed(value, e) {
        if (!value || value.length <= 0) return;

        var range = this.quill.getSelection();
        if (!range) range = {
            index: getContentText(this.quill).length - 1,
            length: 0
        };

        // Unfortunately this line of code needs to be here. I don't understand
        // why it is necessary to format the URL when adding video as embed, as
        // it should already do this automatically ...
        if (this.format === 'video') {
            value = extractVideoUrl(value);
        }

        this.quill.insertEmbed(
            range.index,
            this.format,
            value);
    }
}