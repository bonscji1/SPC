import { Editor } from '@tiptap/core';
import StarterKit from '@tiptap/starter-kit';
import Mention from '@tiptap/extension-mention';
import { createTiptapMentionSuggestion } from './mention-picker.js';

const IngredientMention = Mention.extend({
  name: 'ingredientMention',

  addAttributes() {
    return {
      id: {
        default: null,
        parseHTML: (element) => element.getAttribute('data-id'),
        renderHTML: (attributes) => (attributes.id ? { 'data-id': attributes.id } : {}),
      },
      label: {
        default: null,
        parseHTML: (element) => element.getAttribute('data-label'),
        renderHTML: (attributes) => (attributes.label ? { 'data-label': attributes.label } : {}),
      },
      kind: {
        default: 'ingredient',
        parseHTML: (element) => element.getAttribute('data-kind'),
        renderHTML: (attributes) => ({ 'data-kind': attributes.kind ?? 'ingredient' }),
      },
      detail: {
        default: null,
        parseHTML: (element) => element.getAttribute('data-detail'),
        renderHTML: (attributes) => {
          if (!attributes.detail) {
            return {};
          }

          return {
            'data-detail': attributes.detail,
            title: attributes.detail,
          };
        },
      },
    };
  },

  renderHTML({ node, HTMLAttributes }) {
    const label = node.attrs.label ?? 'ingredient';
    return [
      'span',
      {
        ...HTMLAttributes,
        class: 'instruction-chip editor-mention',
        contenteditable: 'false',
      },
      ['span', { class: 'instruction-chip-label' }, label],
      node.attrs.detail
        ? ['span', { class: 'instruction-chip-tip', role: 'tooltip' }, node.attrs.detail]
        : '',
    ];
  },

  renderText({ node }) {
    return node.attrs.label ?? '';
  },
});

const emptyDoc = {
  type: 'doc',
  content: [{ type: 'paragraph' }],
};

const instances = new Map();

function parseContent(content) {
  if (!content) {
    return emptyDoc;
  }

  if (typeof content === 'string') {
    try {
      return JSON.parse(content);
    } catch {
      return emptyDoc;
    }
  }

  return content;
}

function bindToolbar(toolbar, editor) {
  if (!toolbar) {
    return;
  }

  toolbar.querySelectorAll('[data-cmd]').forEach((button) => {
    button.addEventListener('click', () => {
      const command = button.dataset.cmd;
      if (command === 'bold') {
        editor.chain().focus().toggleBold().run();
      } else if (command === 'italic') {
        editor.chain().focus().toggleItalic().run();
      } else if (command === 'bulletList') {
        editor.chain().focus().toggleBulletList().run();
      }
    });
  });
}

export function initStepEditor(host, toolbar, content, mentionItems, dotnetRef) {
  if (!host) {
    return null;
  }

  const state = {
    mentionItems: mentionItems ?? [],
    dotnetRef,
  };

  const contentHost = document.createElement('div');
  contentHost.className = 'rich-editor-content ProseMirror-host';
  host.appendChild(contentHost);

  const editor = new Editor({
    element: contentHost,
    extensions: [
      StarterKit.configure({
        heading: false,
        blockquote: false,
        codeBlock: false,
        horizontalRule: false,
      }),
      IngredientMention.configure({
        HTMLAttributes: {
          class: 'instruction-chip editor-mention',
        },
        suggestion: createTiptapMentionSuggestion(() => state.mentionItems),
      }),
    ],
    content: parseContent(content),
    editorProps: {
      attributes: {
        class: 'rich-editor-surface',
        spellcheck: 'true',
      },
    },
    onUpdate: ({ editor: currentEditor }) => {
      if (!state.dotnetRef) {
        return;
      }

      const json = JSON.stringify(currentEditor.getJSON());
      state.dotnetRef.invokeMethodAsync('OnEditorChange', json);
    },
  });

  bindToolbar(toolbar, editor);

  const id = `step-${instances.size + 1}`;
  instances.set(id, { editor, host, state });
  return id;
}

export function updateMentions(id, mentionItems) {
  const instance = instances.get(id);
  if (!instance) {
    return;
  }

  instance.state.mentionItems = mentionItems ?? [];
}

export function destroyStepEditor(id) {
  const instance = instances.get(id);
  if (!instance) {
    return;
  }

  instance.editor.destroy();
  instance.host.replaceChildren();
  instances.delete(id);
}
