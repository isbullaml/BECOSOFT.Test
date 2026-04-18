FroalaEditor.PLUGINS['fixPasteBug'] = function (editor) {
  // Fix froala's `id="isPasted"` being added during paste event, potentially causing accessibility and duplicate render issues.
  // https://github.com/froala/wysiwyg-editor/issues/4328
  return {
      _init() {
          editor.events.on('paste.after',
              () => {
                  editor.selection.save();
                  const t = document.createElement('template');
                  t.innerHTML = editor.html.get(true);
                  const offender = t.content.querySelector('#isPasted');
                  if (offender) {
                      offender.removeAttribute('id');
                      editor.html.set(t.innerHTML);
                  }
                  // restore is required after calling editor.selection.save(), otherwise onModelChange won't trigger
                  editor.selection.restore();
              });
      }
  };
};