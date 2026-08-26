function escapeHtml(value) {
  return String(value)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;');
}

function positionPicker(element, clientRect) {
  if (!clientRect) {
    return;
  }

  const rect = typeof clientRect === 'function' ? clientRect() : clientRect;
  if (!rect) {
    return;
  }

  element.style.position = 'fixed';
  element.style.left = `${Math.max(8, rect.left)}px`;
  element.style.top = `${rect.bottom + 6}px`;
  element.style.zIndex = '1000';
  element.style.minWidth = `${Math.max(220, rect.width)}px`;
}

function renderPickerList(element, items, selectedIndex, onPick) {
  element.innerHTML = '';
  const list = document.createElement('ul');
  list.className = 'instruction-picker-list';
  list.setAttribute('role', 'listbox');

  items.forEach((item, index) => {
    const row = document.createElement('li');
    const button = document.createElement('button');
    button.type = 'button';
    button.className = `instruction-picker-item${index === selectedIndex ? ' is-active' : ''}`;
    button.setAttribute('role', 'option');
    button.setAttribute('aria-selected', index === selectedIndex ? 'true' : 'false');
    button.innerHTML = `<span>${escapeHtml(item.label)}</span><span class="editor-picker-meta">${escapeHtml(item.kind)} · ${escapeHtml(item.detail ?? '')}</span>`;
    button.addEventListener('mousedown', (event) => {
      event.preventDefault();
      event.stopPropagation();
      onPick(item);
    });
    row.appendChild(button);
    list.appendChild(row);
  });

  element.appendChild(list);
}

function updatePickerSelection(element, selectedIndex) {
  const buttons = element.querySelectorAll('.instruction-picker-item');
  buttons.forEach((button, index) => {
    const active = index === selectedIndex;
    button.classList.toggle('is-active', active);
    button.setAttribute('aria-selected', active ? 'true' : 'false');
  });

  const activeButton = buttons[selectedIndex];
  if (activeButton) {
    activeButton.scrollIntoView({ block: 'nearest' });
  }
}

function stopPickerKeys(event) {
  event.preventDefault();
  event.stopPropagation();
}

export function createTiptapMentionSuggestion(getItems) {
  return {
    char: '#',
    allowSpaces: false,
    command: ({ editor, range, props }) => {
      editor
        .chain()
        .focus()
        .insertContentAt(range, {
          type: 'ingredientMention',
          attrs: {
            ...props,
            mentionSuggestionChar: '#',
          },
        })
        .run();
    },
    items: ({ query }) => {
      const normalized = query.toLowerCase();
      return getItems()
        .filter((item) => item.name.toLowerCase().includes(normalized))
        .slice(0, 8)
        .map((item) => ({
          id: String(item.id),
          label: item.name,
          kind: item.kind,
          detail: item.detail ?? '',
        }));
    },
    render: () => {
      let element = null;
      let selectedIndex = 0;
      let currentItems = [];
      let currentQuery = '';
      let suggestionProps = null;

      const pick = (item) => {
        suggestionProps?.command?.(item);
      };

      return {
        onStart: (props) => {
          suggestionProps = props;
          element = document.createElement('div');
          element.className = 'instruction-picker editor-mention-picker';
          document.body.appendChild(element);
          selectedIndex = 0;
          currentQuery = props.query ?? '';
          currentItems = props.items;
          renderPickerList(element, currentItems, selectedIndex, pick);
          positionPicker(element, props.clientRect);
        },
        onUpdate: (props) => {
          suggestionProps = props;
          if ((props.query ?? '') !== currentQuery) {
            selectedIndex = 0;
            currentQuery = props.query ?? '';
            currentItems = props.items;
            renderPickerList(element, currentItems, selectedIndex, pick);
          } else {
            currentItems = props.items;
          }

          positionPicker(element, props.clientRect);
        },
        onKeyDown: ({ event }) => {
          if (event.key === 'ArrowUp') {
            stopPickerKeys(event);
            selectedIndex = (selectedIndex + currentItems.length - 1) % currentItems.length;
            updatePickerSelection(element, selectedIndex);
            return true;
          }

          if (event.key === 'ArrowDown') {
            stopPickerKeys(event);
            selectedIndex = (selectedIndex + 1) % currentItems.length;
            updatePickerSelection(element, selectedIndex);
            return true;
          }

          if (event.key === 'Enter') {
            stopPickerKeys(event);
            const item = currentItems[selectedIndex];
            if (item) {
              pick(item);
            }
            return true;
          }

          if (event.key === 'Escape') {
            stopPickerKeys(event);
            return true;
          }

          return false;
        },
        onExit: () => {
          suggestionProps = null;
          element?.remove();
          element = null;
          selectedIndex = 0;
          currentItems = [];
          currentQuery = '';
        },
      };
    },
  };
}
