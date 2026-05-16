import { useState, useRef } from 'react';
import { Plus, Link, Upload, Trash2, Pencil, Check, X, GripVertical, Loader2, Image as ImageIcon } from 'lucide-react';
import { cn } from '../../lib/utils';
import type { ProductImage } from '../../types/product';
import { useProductImages } from '../../hooks/useProducts';

type Props = {
  productId: string;
};

type Tab = 'url' | 'upload';
type EditingImage = { id: string; title: string; description: string };

export function ProductImageManager({ productId }: Props) {
  const [tab, setTab] = useState<Tab>('url');
  const [urlForm, setUrlForm] = useState({ title: '', url: '', description: '' });
  const [uploadForm, setUploadForm] = useState({ title: '', description: '', file: null as File | null });
  const [editingImage, setEditingImage] = useState<EditingImage | null>(null);
  const fileRef = useRef<HTMLInputElement>(null);

  const { query, addByUrl, addByUpload, updateImage, deleteImage } = useProductImages(productId);
  const images = query.data ?? [];
  const isAdding = addByUrl.isPending || addByUpload.isPending;

  async function handleAddByUrl(e: React.FormEvent) {
    e.preventDefault();
    if (!urlForm.title.trim() || !urlForm.url.trim()) return;
    await addByUrl.mutateAsync({
      title: urlForm.title.trim(),
      url: urlForm.url.trim(),
      description: urlForm.description.trim() || undefined,
    });
    setUrlForm({ title: '', url: '', description: '' });
  }

  async function handleAddByUpload(e: React.FormEvent) {
    e.preventDefault();
    if (!uploadForm.title.trim() || !uploadForm.file) return;
    await addByUpload.mutateAsync({
      file: uploadForm.file,
      title: uploadForm.title.trim(),
      description: uploadForm.description.trim() || undefined,
    });
    setUploadForm({ title: '', description: '', file: null });
    if (fileRef.current) fileRef.current.value = '';
  }

  async function handleSaveEdit() {
    if (!editingImage) return;
    await updateImage.mutateAsync({
      imageId: editingImage.id,
      title: editingImage.title.trim(),
      description: editingImage.description.trim() || undefined,
    });
    setEditingImage(null);
  }

  const inputCls = 'w-full rounded-lg border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 px-3 py-2 text-sm text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 transition';

  return (
    <div className="space-y-4">
      {/* Add form */}
      <div className="border border-gray-200 dark:border-gray-700 rounded-xl overflow-hidden">
        <div className="flex border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800">
          {(['url', 'upload'] as Tab[]).map((t) => (
            <button
              key={t}
              type="button"
              onClick={() => setTab(t)}
              className={cn(
                'flex items-center gap-1.5 px-4 py-2.5 text-sm font-medium transition-colors',
                tab === t
                  ? 'border-b-2 border-primary-700 text-primary-700 dark:text-primary-400 bg-white dark:bg-gray-900'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200',
              )}
            >
              {t === 'url' ? <Link className="w-3.5 h-3.5" /> : <Upload className="w-3.5 h-3.5" />}
              {t === 'url' ? 'Add by URL' : 'Upload File'}
            </button>
          ))}
        </div>

        <div className="p-4">
          {tab === 'url' ? (
            <form onSubmit={handleAddByUrl} className="space-y-3">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Title *</label>
                  <input
                    value={urlForm.title}
                    onChange={(e) => setUrlForm((f) => ({ ...f, title: e.target.value }))}
                    placeholder="Front view"
                    className={inputCls}
                    disabled={isAdding}
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Image URL *</label>
                  <input
                    value={urlForm.url}
                    onChange={(e) => setUrlForm((f) => ({ ...f, url: e.target.value }))}
                    placeholder="https://example.com/image.jpg"
                    className={inputCls}
                    disabled={isAdding}
                  />
                </div>
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Description</label>
                <input
                  value={urlForm.description}
                  onChange={(e) => setUrlForm((f) => ({ ...f, description: e.target.value }))}
                  placeholder="Optional description"
                  className={inputCls}
                  disabled={isAdding}
                />
              </div>
              <button
                type="submit"
                disabled={isAdding || !urlForm.title.trim() || !urlForm.url.trim()}
                className="flex items-center gap-2 px-3 py-1.5 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg disabled:opacity-50 transition-colors"
              >
                {isAdding ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Plus className="w-3.5 h-3.5" />}
                Add Image
              </button>
            </form>
          ) : (
            <form onSubmit={handleAddByUpload} className="space-y-3">
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Title *</label>
                  <input
                    value={uploadForm.title}
                    onChange={(e) => setUploadForm((f) => ({ ...f, title: e.target.value }))}
                    placeholder="Front view"
                    className={inputCls}
                    disabled={isAdding}
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">File * (JPG, PNG, WEBP, GIF)</label>
                  <input
                    ref={fileRef}
                    type="file"
                    accept=".jpg,.jpeg,.png,.webp,.gif"
                    onChange={(e) => setUploadForm((f) => ({ ...f, file: e.target.files?.[0] ?? null }))}
                    className="w-full text-sm text-gray-700 dark:text-gray-300 file:mr-3 file:py-1.5 file:px-3 file:rounded-lg file:border-0 file:text-sm file:font-medium file:bg-primary-50 file:text-primary-700 dark:file:bg-primary-900/20 dark:file:text-primary-400 hover:file:bg-primary-100"
                    disabled={isAdding}
                  />
                </div>
              </div>
              <div>
                <label className="block text-xs font-medium text-gray-600 dark:text-gray-400 mb-1">Description</label>
                <input
                  value={uploadForm.description}
                  onChange={(e) => setUploadForm((f) => ({ ...f, description: e.target.value }))}
                  placeholder="Optional description"
                  className={inputCls}
                  disabled={isAdding}
                />
              </div>
              <button
                type="submit"
                disabled={isAdding || !uploadForm.title.trim() || !uploadForm.file}
                className="flex items-center gap-2 px-3 py-1.5 bg-primary-800 hover:bg-primary-900 text-white text-sm font-medium rounded-lg disabled:opacity-50 transition-colors"
              >
                {isAdding ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Upload className="w-3.5 h-3.5" />}
                Upload & Add
              </button>
            </form>
          )}
        </div>
      </div>

      {/* Image list */}
      {query.isLoading ? (
        <div className="text-center py-6">
          <Loader2 className="w-5 h-5 animate-spin text-gray-400 mx-auto" />
        </div>
      ) : images.length === 0 ? (
        <div className="flex flex-col items-center gap-2 py-8 text-gray-400">
          <ImageIcon className="w-8 h-8" />
          <p className="text-sm">No images yet. Add one above.</p>
        </div>
      ) : (
        <div className="space-y-2">
          {images.map((img) => (
            <ImageRow
              key={img.id}
              image={img}
              editing={editingImage?.id === img.id ? editingImage : null}
              onEdit={() => setEditingImage({ id: img.id, title: img.title, description: img.description ?? '' })}
              onEditChange={(field, val) => setEditingImage((e) => e ? { ...e, [field]: val } : e)}
              onSaveEdit={handleSaveEdit}
              onCancelEdit={() => setEditingImage(null)}
              onDelete={() => deleteImage.mutateAsync(img.id)}
              isSaving={updateImage.isPending && editingImage?.id === img.id}
              isDeleting={deleteImage.isPending}
            />
          ))}
        </div>
      )}
    </div>
  );
}

type ImageRowProps = {
  image: ProductImage;
  editing: EditingImage | null;
  onEdit: () => void;
  onEditChange: (field: 'title' | 'description', val: string) => void;
  onSaveEdit: () => void;
  onCancelEdit: () => void;
  onDelete: () => void;
  isSaving: boolean;
  isDeleting: boolean;
};

function ImageRow({
  image, editing, onEdit, onEditChange, onSaveEdit, onCancelEdit, onDelete, isSaving, isDeleting,
}: ImageRowProps) {
  const inputCls = 'w-full rounded border border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 px-2 py-1 text-xs text-gray-900 dark:text-white focus:outline-none focus-visible:ring-1 focus-visible:ring-primary-500';

  return (
    <div className="flex items-start gap-3 p-3 border border-gray-200 dark:border-gray-700 rounded-lg bg-white dark:bg-gray-900 group">
      <GripVertical className="w-4 h-4 text-gray-300 dark:text-gray-600 mt-1 flex-shrink-0 cursor-grab" />

      {/* Thumbnail */}
      <div className="w-12 h-12 rounded-md border border-gray-100 dark:border-gray-700 overflow-hidden flex-shrink-0 bg-gray-50 dark:bg-gray-800">
        <img
          src={image.url}
          alt={image.title}
          className="w-full h-full object-cover"
          onError={(e) => { (e.target as HTMLImageElement).style.display = 'none'; }}
        />
      </div>

      {/* Content */}
      {editing ? (
        <div className="flex-1 space-y-1.5">
          <input
            value={editing.title}
            onChange={(e) => onEditChange('title', e.target.value)}
            placeholder="Title"
            className={inputCls}
            disabled={isSaving}
          />
          <input
            value={editing.description}
            onChange={(e) => onEditChange('description', e.target.value)}
            placeholder="Description (optional)"
            className={inputCls}
            disabled={isSaving}
          />
        </div>
      ) : (
        <div className="flex-1 min-w-0">
          <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{image.title}</p>
          {image.description && (
            <p className="text-xs text-gray-500 dark:text-gray-400 truncate">{image.description}</p>
          )}
          <div className="flex items-center gap-2 mt-0.5">
            <span className={cn(
              'text-xs px-1.5 py-0.5 rounded',
              image.isUploaded
                ? 'bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400'
                : 'bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400',
            )}>
              {image.isUploaded ? 'Uploaded' : 'URL'}
            </span>
            <span className="text-xs text-gray-400 truncate max-w-[180px]">{image.url}</span>
          </div>
        </div>
      )}

      {/* Actions */}
      <div className="flex items-center gap-1 flex-shrink-0">
        {editing ? (
          <>
            <button
              type="button"
              onClick={onSaveEdit}
              disabled={isSaving}
              className="p-1.5 rounded text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20 transition-colors disabled:opacity-40"
            >
              {isSaving ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Check className="w-3.5 h-3.5" />}
            </button>
            <button
              type="button"
              onClick={onCancelEdit}
              className="p-1.5 rounded text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            >
              <X className="w-3.5 h-3.5" />
            </button>
          </>
        ) : (
          <>
            <button
              type="button"
              onClick={onEdit}
              className="p-1.5 rounded text-gray-400 hover:text-primary-700 hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors opacity-0 group-hover:opacity-100"
            >
              <Pencil className="w-3.5 h-3.5" />
            </button>
            <button
              type="button"
              onClick={onDelete}
              disabled={isDeleting}
              className="p-1.5 rounded text-gray-400 hover:text-red-600 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors opacity-0 group-hover:opacity-100 disabled:opacity-40"
            >
              {isDeleting ? <Loader2 className="w-3.5 h-3.5 animate-spin" /> : <Trash2 className="w-3.5 h-3.5" />}
            </button>
          </>
        )}
      </div>
    </div>
  );
}
