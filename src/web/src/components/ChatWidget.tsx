import { useEffect, useRef, useState } from 'react';
import { chatApi } from '../api/endpoints';
import type { ChatMessage } from '../api/types';
import './ChatWidget.css';

/**
 * A permanently-visible AI assistant. A round button sits in the bottom-left corner; clicking it
 * opens a chat panel that answers questions grounded in the app's data (via the /api/chat endpoint,
 * backed by Azure OpenAI gpt-5.3-chat). Hovering the button reveals a label.
 */
/** Default and bounds (px) for the resizable panel. */
const DEFAULT_SIZE = { width: 360, height: 460 };
const MIN_SIZE = { width: 300, height: 320 };

export function ChatWidget() {
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [busy, setBusy] = useState(false);
  const [size, setSize] = useState(DEFAULT_SIZE);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Keep the newest message in view.
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages, busy]);

  // Drag the top-left grip to resize. The panel is pinned bottom-right, so growing means
  // dragging up and to the left — width/height increase as the cursor moves that way.
  function startResize(event: React.MouseEvent) {
    event.preventDefault();
    const startX = event.clientX;
    const startY = event.clientY;
    const startWidth = size.width;
    const startHeight = size.height;

    function onMove(moveEvent: MouseEvent) {
      const maxWidth = window.innerWidth - 48;
      const maxHeight = window.innerHeight - 128;
      const width = Math.min(maxWidth, Math.max(MIN_SIZE.width, startWidth + (startX - moveEvent.clientX)));
      const height = Math.min(maxHeight, Math.max(MIN_SIZE.height, startHeight + (startY - moveEvent.clientY)));
      setSize({ width, height });
    }
    function onUp() {
      window.removeEventListener('mousemove', onMove);
      window.removeEventListener('mouseup', onUp);
    }
    window.addEventListener('mousemove', onMove);
    window.addEventListener('mouseup', onUp);
  }

  async function send() {
    const text = input.trim();
    if (!text || busy) {
      return;
    }
    const nextMessages: ChatMessage[] = [...messages, { role: 'user', content: text }];
    setMessages(nextMessages);
    setInput('');
    setBusy(true);
    try {
      const reply = await chatApi.ask(nextMessages);
      setMessages([...nextMessages, { role: 'assistant', content: reply.reply }]);
    } catch {
      setMessages([...nextMessages, { role: 'assistant', content: 'Sorry — the assistant is unreachable right now.' }]);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="chat-widget">
      {open && (
        <div
          className="chat-panel"
          role="dialog"
          aria-label="AID Dashboard assistant"
          style={{ width: size.width, height: size.height }}
        >
          <div className="chat-resize" onMouseDown={startResize} title="Drag to resize" aria-hidden="true" />
          <div className="chat-header">
            <span>AID Assistant</span>
            <button type="button" className="chat-close" aria-label="Close" onClick={() => setOpen(false)}>
              ×
            </button>
          </div>

          <div className="chat-messages">
            {messages.length === 0 && (
              <div className="chat-hint">
                Ask me about the accounts and their automations — for example, “Which automations use Sophie?” or
                “What’s the total estimated saving?”
              </div>
            )}
            {messages.map((message, index) => (
              <div key={index} className={`chat-msg chat-msg-${message.role}`}>
                {message.content}
              </div>
            ))}
            {busy && <div className="chat-msg chat-msg-assistant chat-typing">…</div>}
            <div ref={messagesEndRef} />
          </div>

          <div className="chat-input">
            <textarea
              value={input}
              placeholder="Type a question…"
              rows={2}
              onChange={(event) => setInput(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === 'Enter' && !event.shiftKey) {
                  event.preventDefault();
                  send();
                }
              }}
            />
            <button type="button" className="btn-primary" disabled={busy || !input.trim()} onClick={send}>
              Send
            </button>
          </div>
        </div>
      )}

      <button
        type="button"
        className="chat-fab"
        title="Ask the AID Dashboard assistant"
        aria-label="Open the AID Dashboard assistant"
        onClick={() => setOpen((value) => !value)}
      >
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" aria-hidden="true">
          <path
            d="M4 4h16a1 1 0 0 1 1 1v11a1 1 0 0 1-1 1H9l-4 4v-4H4a1 1 0 0 1-1-1V5a1 1 0 0 1 1-1Z"
            fill="currentColor"
          />
          <circle cx="8.5" cy="10.5" r="1.2" fill="#fff" />
          <circle cx="12" cy="10.5" r="1.2" fill="#fff" />
          <circle cx="15.5" cy="10.5" r="1.2" fill="#fff" />
        </svg>
        <span className="chat-fab-label">Ask AID</span>
      </button>
    </div>
  );
}
