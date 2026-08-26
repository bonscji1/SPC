import { destroyStepEditor, initStepEditor, updateMentions } from './tiptap-step-editor.js';

window.spcInstructionEditor = {
  init: initStepEditor,
  updateMentions,
  destroy: destroyStepEditor,
};
