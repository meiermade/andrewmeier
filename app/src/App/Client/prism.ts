import Prism from 'prismjs'
import 'prismjs/components/prism-bash'
import 'prismjs/components/prism-fsharp'
import 'prismjs/components/prism-ini'
import 'prismjs/components/prism-json'
import 'prismjs/components/prism-typescript'
import 'prismjs/components/prism-yaml'
import 'prismjs/plugins/toolbar/prism-toolbar'
import 'prismjs/plugins/show-language/prism-show-language'
import 'prismjs/plugins/copy-to-clipboard/prism-copy-to-clipboard'

Prism.manual = true

const highlightAll = () => Prism.highlightAll()

if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', highlightAll, { once: true })
} else {
  highlightAll()
}
