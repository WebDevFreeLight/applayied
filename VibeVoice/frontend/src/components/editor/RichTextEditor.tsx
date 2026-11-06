import Link from '@tiptap/extension-link'
import Placeholder from '@tiptap/extension-placeholder'
import TextAlign from '@tiptap/extension-text-align'
import { EditorContent, useEditor, useEditorState, type Editor } from '@tiptap/react'
import StarterKit from '@tiptap/starter-kit'
import {
  AlignCenter,
  AlignLeft,
  AlignRight,
  Bold,
  Code2,
  Heading1,
  Heading2,
  Heading3,
  Italic,
  Link2,
  List,
  ListOrdered,
  Quote,
  Redo2,
  Strikethrough,
  Undo2,
  type LucideIcon,
} from 'lucide-react'
import { useImperativeHandle, type Ref } from 'react'
import { useTranslation } from 'react-i18next'
import { useLatest } from '@/hooks/misc'
import { cn } from '@/lib/utils'

export interface RichTextHandle {
  appendHtml: (html: string) => void
  focus: () => void
  getHtml: () => string
}

export interface RichTextEditorProps {
  /** Initial HTML (the editor is uncontrolled afterwards). */
  initialHtml: string
  onChange: (html: string) => void
  placeholder?: string
  label: string
  handleRef?: Ref<RichTextHandle>
  className?: string
}

function htmlOf(editor: Editor): string {
  return editor.isEmpty ? '' : editor.getHTML()
}

export function RichTextEditor({
  initialHtml,
  onChange,
  placeholder,
  label,
  handleRef,
  className,
}: RichTextEditorProps) {
  const onChangeRef = useLatest(onChange)
  const editor = useEditor({
    extensions: [
      StarterKit.configure({ link: false, heading: { levels: [1, 2, 3] } }),
      Link.configure({
        openOnClick: false,
        autolink: true,
        defaultProtocol: 'https',
        HTMLAttributes: { rel: 'noopener noreferrer nofollow', target: '_blank' },
      }),
      TextAlign.configure({ types: ['heading', 'paragraph'] }),
      Placeholder.configure({ placeholder: placeholder ?? '' }),
    ],
    content: initialHtml || '',
    immediatelyRender: true,
    shouldRerenderOnTransaction: false,
    editorProps: {
      attributes: {
        dir: 'auto',
        role: 'textbox',
        'aria-multiline': 'true',
        'aria-label': label,
        class: 'rich-text min-h-[14rem] px-4 py-3 sm:px-5 text-fg',
      },
    },
    onUpdate: ({ editor: e }) => onChangeRef.current(htmlOf(e)),
  })

  useImperativeHandle(
    handleRef,
    () => ({
      appendHtml: (html: string) => {
        if (!editor) return
        editor.chain().focus('end').insertContent(html).run()
      },
      focus: () => editor?.commands.focus(),
      getHtml: () => (editor ? htmlOf(editor) : ''),
    }),
    [editor],
  )

  return (
    <div
      className={cn(
        'overflow-hidden rounded-2xl border border-border bg-surface transition-colors focus-within:border-accent focus-within:ring-2 focus-within:ring-accent/20',
        className,
      )}
    >
      {editor && <Toolbar editor={editor} />}
      <EditorContent editor={editor} />
    </div>
  )
}

function Toolbar({ editor }: { editor: Editor }) {
  const { t } = useTranslation()
  const state = useEditorState({
    editor,
    selector: ({ editor: e }) => ({
      bold: e.isActive('bold'),
      italic: e.isActive('italic'),
      strike: e.isActive('strike'),
      h1: e.isActive('heading', { level: 1 }),
      h2: e.isActive('heading', { level: 2 }),
      h3: e.isActive('heading', { level: 3 }),
      bullet: e.isActive('bulletList'),
      ordered: e.isActive('orderedList'),
      quote: e.isActive('blockquote'),
      code: e.isActive('codeBlock'),
      link: e.isActive('link'),
      left: e.isActive({ textAlign: 'left' }),
      center: e.isActive({ textAlign: 'center' }),
      right: e.isActive({ textAlign: 'right' }),
      canUndo: e.can().undo(),
      canRedo: e.can().redo(),
    }),
  })

  const rtl = document.documentElement.dir === 'rtl'
  const setLink = () => {
    const prev = editor.getAttributes('link').href as string | undefined
    const url = window.prompt(t('editor.linkPrompt'), prev ?? 'https://')
    if (url === null) return
    if (!url.trim() || url.trim() === 'https://') {
      editor.chain().focus().extendMarkRange('link').unsetLink().run()
      return
    }
    editor.chain().focus().extendMarkRange('link').setLink({ href: url.trim() }).run()
  }

  type Item = { icon: LucideIcon; label: string; active?: boolean; disabled?: boolean; run: () => void }
  const groups: Item[][] = [
    [
      { icon: Bold, label: t('editor.bold'), active: state.bold, run: () => editor.chain().focus().toggleBold().run() },
      { icon: Italic, label: t('editor.italic'), active: state.italic, run: () => editor.chain().focus().toggleItalic().run() },
      { icon: Strikethrough, label: t('editor.strike'), active: state.strike, run: () => editor.chain().focus().toggleStrike().run() },
    ],
    [
      { icon: Heading1, label: t('editor.heading', { level: 1 }), active: state.h1, run: () => editor.chain().focus().toggleHeading({ level: 1 }).run() },
      { icon: Heading2, label: t('editor.heading', { level: 2 }), active: state.h2, run: () => editor.chain().focus().toggleHeading({ level: 2 }).run() },
      { icon: Heading3, label: t('editor.heading', { level: 3 }), active: state.h3, run: () => editor.chain().focus().toggleHeading({ level: 3 }).run() },
    ],
    [
      { icon: List, label: t('editor.bulletList'), active: state.bullet, run: () => editor.chain().focus().toggleBulletList().run() },
      { icon: ListOrdered, label: t('editor.orderedList'), active: state.ordered, run: () => editor.chain().focus().toggleOrderedList().run() },
      { icon: Quote, label: t('editor.quote'), active: state.quote, run: () => editor.chain().focus().toggleBlockquote().run() },
      { icon: Code2, label: t('editor.code'), active: state.code, run: () => editor.chain().focus().toggleCodeBlock().run() },
      { icon: Link2, label: t('editor.link'), active: state.link, run: setLink },
    ],
    [
      // "Start"/"end" follow the UI direction so the buttons mirror correctly in RTL.
      {
        icon: rtl ? AlignRight : AlignLeft,
        label: t('editor.alignStart'),
        active: rtl ? state.right : state.left,
        run: () => editor.chain().focus().setTextAlign(rtl ? 'right' : 'left').run(),
      },
      { icon: AlignCenter, label: t('editor.alignCenter'), active: state.center, run: () => editor.chain().focus().setTextAlign('center').run() },
      {
        icon: rtl ? AlignLeft : AlignRight,
        label: t('editor.alignEnd'),
        active: rtl ? state.left : state.right,
        run: () => editor.chain().focus().setTextAlign(rtl ? 'left' : 'right').run(),
      },
    ],
    [
      { icon: Undo2, label: t('editor.undo'), disabled: !state.canUndo, run: () => editor.chain().focus().undo().run() },
      { icon: Redo2, label: t('editor.redo'), disabled: !state.canRedo, run: () => editor.chain().focus().redo().run() },
    ],
  ]

  return (
    <div
      role="toolbar"
      aria-label={t('editor.toolbar')}
      className="scrollbar-thin flex items-center gap-1 overflow-x-auto border-b border-border bg-surface-2/60 px-2 py-1.5"
    >
      {groups.map((group, gi) => (
        <div key={gi} className="flex items-center gap-0.5 border-border not-last:me-1 not-last:border-e not-last:pe-1">
          {group.map(({ icon: Icon, label, active, disabled, run }) => (
            <button
              key={label}
              type="button"
              title={label}
              aria-label={label}
              aria-pressed={active ?? undefined}
              disabled={disabled}
              onMouseDown={(e) => e.preventDefault()}
              onClick={run}
              className={cn(
                'inline-flex size-8 shrink-0 items-center justify-center rounded-lg transition-colors disabled:opacity-35',
                active ? 'bg-accent-soft text-accent-text' : 'text-muted hover:bg-surface-3 hover:text-fg',
              )}
            >
              <Icon className="size-4" aria-hidden />
            </button>
          ))}
        </div>
      ))}
    </div>
  )
}
