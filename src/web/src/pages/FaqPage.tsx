import { PageHeader } from '../components/PageHeader';
import faqHtml from './faqContent.html?raw';
import './FaqPage.css';

/**
 * Static FAQ page. The content is the body of the source FAQ email (text + screenshots),
 * extracted at build time into faqContent.html; the screenshots are served from /public/faq.
 */
export function FaqPage() {
  return (
    <>
      <PageHeader title="FAQ" />
      <div className="faq-content" dangerouslySetInnerHTML={{ __html: faqHtml }} />
    </>
  );
}
