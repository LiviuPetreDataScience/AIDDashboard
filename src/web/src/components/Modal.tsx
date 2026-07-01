import { type ReactNode } from 'react';
import './Modal.css';

interface ModalProps {
  title: string;
  onClose: () => void;
  children: ReactNode;
}

/** A simple centered modal dialog with an overlay; closes on overlay click or the × button. */
export function Modal({ title, onClose, children }: ModalProps) {
  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal" role="dialog" aria-modal="true" onClick={(event) => event.stopPropagation()}>
        <div className="modal-header">
          <h3>{title}</h3>
          <button type="button" className="modal-close" aria-label="Close" onClick={onClose}>
            ×
          </button>
        </div>
        <div className="modal-body">{children}</div>
      </div>
    </div>
  );
}
