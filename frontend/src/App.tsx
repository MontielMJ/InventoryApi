import { QueryClient, QueryClientProvider, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { BrowserRouter, Link, Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom';
import { useMemo, useState, type FormEvent, type ReactNode } from 'react';
import { AuthProvider } from './auth/AuthProvider';
import { useAuth } from './auth/useAuth';
import { createApiClient } from './api/client';
import { inventoryApi } from './api/inventoryApi';
import type { CategoryInput, MovementInput, ProductInput } from './types';
import './styles.css';

const queryClient = new QueryClient();

function ProtectedRoute() {
  const auth = useAuth();
  if (auth.loading) return <div className="center-page">Loading session...</div>;
  return auth.authenticated ? <Outlet /> : <Navigate to="/login" replace />;
}

function Layout() {
  const auth = useAuth();
  return <div className="app-shell"><aside className="sidebar"><div className="brand">Stock<span>Flow</span></div><nav><NavItem to="/" label="Dashboard" icon="⌂" /><NavItem to="/products" label="Products" icon="▣" /><NavItem to="/categories" label="Categories" icon="◈" /><NavItem to="/movements" label="Movements" icon="↕" /></nav><div className="sidebar-footer"><div className="user-badge">{(auth.username ?? 'U').slice(0, 1).toUpperCase()}</div><div className="user-name">{auth.username ?? 'User'}</div><button className="link-button" onClick={auth.logout}>Sign out</button></div></aside><main className="main-content"><header className="topbar"><div><span className="eyebrow">INVENTORY CONTROL</span><h1>Good afternoon</h1></div><div className="status-pill"><span /> System healthy</div></header><Outlet /></main></div>;
}

function NavItem({ to, label, icon }: { to: string; label: string; icon: string }) {
  const location = useLocation();
  const active = to === '/' ? location.pathname === '/' : location.pathname.startsWith(to);
  return <Link className={active ? 'nav-item active' : 'nav-item'} to={to}><span>{icon}</span>{label}</Link>;
}

function Login() {
  const auth = useAuth();
  return <div className="login-page"><div className="login-card"><div className="brand large">Stock<span>Flow</span></div><div className="login-icon">◈</div><h1>Inventory, in control.</h1><p>Sign in to manage products, categories and stock movements.</p><button className="primary-button full" onClick={auth.login}>Continue with Keycloak</button><small>Secure OAuth2 authentication</small></div></div>;
}

function Dashboard() {
  const api = useInventoryApi();
  const products = useQuery({ queryKey: ['products'], queryFn: () => api.products.list() });
  const categories = useQuery({ queryKey: ['categories'], queryFn: api.categories.list });
  const movements = useQuery({ queryKey: ['movements'], queryFn: api.movements.list });
  const productItems = products.data?.items ?? [];
  const lowStock = productItems.filter(product => product.isActive && product.stock <= product.minimumStock);
  return <section><div className="page-heading"><div><span className="eyebrow">OVERVIEW</span><h2>Dashboard</h2><p className="muted">A clear view of your inventory health.</p></div><Link className="primary-button" to="/movements">Record movement <span>+</span></Link></div><div className="stat-grid"><StatCard label="Total products" value={products.data?.totalCount ?? '—'} icon="▣" tone="blue" /><StatCard label="Categories" value={categories.data?.length ?? '—'} icon="◈" tone="violet" /><StatCard label="Low stock" value={lowStock.length} icon="!" tone="orange" /><StatCard label="Movements" value={movements.data?.totalCount ?? '—'} icon="↕" tone="green" /></div><div className="content-grid"><div className="panel"><PanelTitle title="Low stock alerts" action={<Link to="/products">View all</Link>} />{lowStock.length === 0 ? <EmptyState message="All products have healthy stock." /> : <div className="alert-list">{lowStock.slice(0, 5).map(product => <div className="alert-row" key={product.id}><div className="product-avatar">{product.name.slice(0, 1)}</div><div className="grow"><strong>{product.name}</strong><small>{product.sku}</small></div><span className="stock-warning">{product.stock} / min {product.minimumStock}</span></div>)}</div>}</div><div className="panel"><PanelTitle title="Recent movements" action={<Link to="/movements">View all</Link>} />{movements.data?.items.slice(0, 5).map(movement => <div className="movement-row" key={movement.id}><span className={movement.type === 'In' ? 'movement-icon in' : 'movement-icon out'}>{movement.type === 'In' ? '↓' : '↑'}</span><div className="grow"><strong>{movement.productName}</strong><small>{movement.reason || 'Inventory movement'}</small></div><b className={movement.type === 'In' ? 'text-green' : 'text-red'}>{movement.type === 'In' ? '+' : '-'}{movement.quantity}</b></div>) ?? <EmptyState message="No movements yet." />}</div></div></section>;
}

function Products() {
  const api = useInventoryApi();
  const client = useQueryClient();
  const [search, setSearch] = useStateValue('');
  const [editing, setEditing] = useStateValue<string | null>(null);
  const products = useQuery({ queryKey: ['products', search], queryFn: () => api.products.list(search) });
  const categories = useQuery({ queryKey: ['categories'], queryFn: api.categories.list });
  const save = useMutation({ mutationFn: async (value: ProductInput) => { if (editing && editing !== 'new') await api.products.update(editing, value); else await api.products.create(value); }, onSuccess: () => { client.invalidateQueries({ queryKey: ['products'] }); setEditing(null); } });
  const remove = useMutation({ mutationFn: api.products.remove, onSuccess: () => client.invalidateQueries({ queryKey: ['products'] }) });
  const current = products.data?.items.find(item => item.id === editing);
  return <section><PageHeading title="Products" description="Manage your catalog and monitor stock levels." action={<button className="primary-button" onClick={() => setEditing('new')}>Add product <span>+</span></button>} /><div className="toolbar"><input className="search-input" placeholder="Search by name or SKU..." value={search} onChange={event => setSearch(event.target.value)} /></div>{editing && <ProductForm initial={current} categories={categories.data ?? []} saving={save.isPending} onCancel={() => setEditing(null)} onSubmit={value => save.mutate(value)} />}{products.isLoading ? <Loading /> : <div className="panel table-panel"><table><thead><tr><th>Product</th><th>Category</th><th>Price</th><th>Stock</th><th>Status</th><th /></tr></thead><tbody>{products.data?.items.map(product => <tr key={product.id}><td><div className="table-product"><div className="product-avatar">{product.name.slice(0, 1)}</div><div><strong>{product.name}</strong><small>{product.sku}</small></div></div></td><td>{product.categoryName}</td><td>${product.price.toFixed(2)}</td><td><b className={product.stock <= product.minimumStock ? 'text-orange' : 'text-green'}>{product.stock}</b><small className="table-sub"> min {product.minimumStock}</small></td><td><span className={product.isActive ? 'badge success' : 'badge muted-badge'}>{product.isActive ? 'Active' : 'Inactive'}</span></td><td><button className="icon-button" onClick={() => setEditing(product.id)}>Edit</button><button className="icon-button danger" onClick={() => remove.mutate(product.id)}>Delete</button></td></tr>)}</tbody></table>{products.data?.items.length === 0 && <EmptyState message="No products found." />}</div>}</section>;
}

function Categories() {
  const api = useInventoryApi();
  const client = useQueryClient();
  const [editing, setEditing] = useStateValue<string | null>(null);
  const categories = useQuery({ queryKey: ['categories'], queryFn: api.categories.list });
  const save = useMutation({ mutationFn: async (value: CategoryInput) => { if (editing && editing !== 'new') await api.categories.update(editing, value); else await api.categories.create(value); }, onSuccess: () => { client.invalidateQueries({ queryKey: ['categories'] }); setEditing(null); } });
  const remove = useMutation({ mutationFn: api.categories.remove, onSuccess: () => client.invalidateQueries({ queryKey: ['categories'] }) });
  const current = categories.data?.find(item => item.id === editing);
  return <section><PageHeading title="Categories" description="Organize your inventory with clear categories." action={<button className="primary-button" onClick={() => setEditing('new')}>Add category <span>+</span></button>} />{editing && <CategoryForm initial={current} saving={save.isPending} onCancel={() => setEditing(null)} onSubmit={value => save.mutate(value)} />}<div className="category-grid">{categories.data?.map(category => <div className="category-card" key={category.id}><div className="category-icon">◈</div><div className="category-info"><h3>{category.name}</h3><p>{category.description || 'No description'}</p><span className={category.isActive ? 'badge success' : 'badge muted-badge'}>{category.isActive ? 'Active' : 'Inactive'}</span></div><div className="card-actions"><button className="icon-button" onClick={() => setEditing(category.id)}>Edit</button><button className="icon-button danger" onClick={() => remove.mutate(category.id)}>Delete</button></div></div>)}</div></section>;
}

function Movements() {
  const api = useInventoryApi();
  const client = useQueryClient();
  const [creating, setCreating] = useStateValue(false);
  const products = useQuery({ queryKey: ['products'], queryFn: () => api.products.list() });
  const movements = useQuery({ queryKey: ['movements'], queryFn: api.movements.list });
  const save = useMutation({ mutationFn: api.movements.create, onSuccess: () => { client.invalidateQueries({ queryKey: ['movements'] }); client.invalidateQueries({ queryKey: ['products'] }); setCreating(false); } });
  return <section><PageHeading title="Movements" description="Track every stock change with a complete audit trail." action={<button className="primary-button" onClick={() => setCreating(true)}>Record movement <span>+</span></button>} />{creating && <MovementForm products={products.data?.items ?? []} saving={save.isPending} onCancel={() => setCreating(false)} onSubmit={value => save.mutate(value)} />}<div className="panel table-panel"><table><thead><tr><th>Product</th><th>Type</th><th>Quantity</th><th>Unit cost</th><th>Reason</th><th>Created by</th><th>Date</th></tr></thead><tbody>{movements.data?.items.map(movement => <tr key={movement.id}><td><strong>{movement.productName}</strong></td><td><span className={movement.type === 'In' ? 'badge success' : 'badge out-badge'}>{movement.type === 'In' ? 'Stock in' : 'Stock out'}</span></td><td className="bold">{movement.quantity}</td><td>${movement.unitCost.toFixed(2)}</td><td>{movement.reason || '—'}</td><td>{movement.createdBy}</td><td>{new Date(movement.createdAt).toLocaleDateString()}</td></tr>)}</tbody></table>{movements.data?.items.length === 0 && <EmptyState message="No movements recorded yet." />}</div></section>;
}

function ProductForm({ initial, categories, saving, onCancel, onSubmit }: { initial?: ProductInput & { id?: string }; categories: { id: string; name: string }[]; saving: boolean; onCancel: () => void; onSubmit: (value: ProductInput) => void }) {
  const [form, setForm] = useStateValue<ProductInput>({ sku: initial?.sku ?? '', name: initial?.name ?? '', description: initial?.description ?? '', categoryId: initial?.categoryId ?? '', price: initial?.price ?? 0, minimumStock: initial?.minimumStock ?? 0 });
  return <FormCard title={initial ? 'Edit product' : 'New product'} onCancel={onCancel} onSubmit={event => { event.preventDefault(); onSubmit(form); }} saving={saving}><div className="form-grid"><Field label="SKU"><input required value={form.sku} onChange={event => setForm({ ...form, sku: event.target.value })} /></Field><Field label="Name"><input required value={form.name} onChange={event => setForm({ ...form, name: event.target.value })} /></Field><Field label="Category"><select required value={form.categoryId} onChange={event => setForm({ ...form, categoryId: event.target.value })}><option value="">Select category</option>{categories.map(category => <option value={category.id} key={category.id}>{category.name}</option>)}</select></Field><Field label="Price"><input required type="number" min="0.01" step="0.01" value={form.price} onChange={event => setForm({ ...form, price: Number(event.target.value) })} /></Field><Field label="Minimum stock"><input required type="number" min="0" value={form.minimumStock} onChange={event => setForm({ ...form, minimumStock: Number(event.target.value) })} /></Field><Field label="Description"><input value={form.description} onChange={event => setForm({ ...form, description: event.target.value })} /></Field></div></FormCard>;
}

function CategoryForm({ initial, saving, onCancel, onSubmit }: { initial?: CategoryInput; saving: boolean; onCancel: () => void; onSubmit: (value: CategoryInput) => void }) { const [form, setForm] = useStateValue<CategoryInput>({ name: initial?.name ?? '', description: initial?.description ?? '' }); return <FormCard title={initial ? 'Edit category' : 'New category'} onCancel={onCancel} onSubmit={event => { event.preventDefault(); onSubmit(form); }} saving={saving}><div className="form-grid"><Field label="Name"><input required value={form.name} onChange={event => setForm({ ...form, name: event.target.value })} /></Field><Field label="Description"><input value={form.description} onChange={event => setForm({ ...form, description: event.target.value })} /></Field></div></FormCard>; }

function MovementForm({ products, saving, onCancel, onSubmit }: { products: { id: string; name: string }[]; saving: boolean; onCancel: () => void; onSubmit: (value: MovementInput) => void }) { const [form, setForm] = useStateValue<MovementInput>({ productId: '', type: 'In', quantity: 1, unitCost: 0, reason: '', reference: '' }); return <FormCard title="Record movement" onCancel={onCancel} onSubmit={event => { event.preventDefault(); onSubmit(form); }} saving={saving}><div className="form-grid"><Field label="Product"><select required value={form.productId} onChange={event => setForm({ ...form, productId: event.target.value })}><option value="">Select product</option>{products.map(product => <option key={product.id} value={product.id}>{product.name}</option>)}</select></Field><Field label="Type"><select value={form.type} onChange={event => setForm({ ...form, type: event.target.value as 'In' | 'Out' })}><option value="In">Stock in</option><option value="Out">Stock out</option></select></Field><Field label="Quantity"><input required type="number" min="1" value={form.quantity} onChange={event => setForm({ ...form, quantity: Number(event.target.value) })} /></Field><Field label="Unit cost"><input required type="number" min="0" step="0.01" value={form.unitCost} onChange={event => setForm({ ...form, unitCost: Number(event.target.value) })} /></Field><Field label="Reason"><input value={form.reason} onChange={event => setForm({ ...form, reason: event.target.value })} /></Field><Field label="Reference"><input value={form.reference} onChange={event => setForm({ ...form, reference: event.target.value })} /></Field></div></FormCard>; }

function FormCard({ title, onCancel, onSubmit, saving, children }: { title: string; onCancel: () => void; onSubmit: (event: FormEvent<HTMLFormElement>) => void; saving: boolean; children: ReactNode }) { return <form className="panel form-card" onSubmit={onSubmit}><div className="form-title"><h3>{title}</h3><button type="button" className="close-button" onClick={onCancel}>×</button></div>{children}<div className="form-actions"><button type="button" className="secondary-button" onClick={onCancel}>Cancel</button><button className="primary-button" disabled={saving}>{saving ? 'Saving...' : 'Save'}</button></div></form>; }
function Field({ label, children }: { label: string; children: ReactNode }) { return <label className="field"><span>{label}</span>{children}</label>; }
function PageHeading({ title, description, action }: { title: string; description: string; action: React.ReactNode }) { return <div className="page-heading"><div><span className="eyebrow">INVENTORY</span><h2>{title}</h2><p className="muted">{description}</p></div>{action}</div>; }
function PanelTitle({ title, action }: { title: string; action?: React.ReactNode }) { return <div className="panel-title"><h3>{title}</h3>{action}</div>; }
function StatCard({ label, value, icon, tone }: { label: string; value: string | number; icon: string; tone: string }) { return <div className="stat-card"><div className={`stat-icon ${tone}`}>{icon}</div><div><span>{label}</span><strong>{value}</strong></div></div>; }
function EmptyState({ message }: { message: string }) { return <div className="empty-state">{message}</div>; }
function Loading() { return <div className="panel empty-state">Loading...</div>; }
function useStateValue<T>(initial: T): [T, (value: T) => void] { const [value, setValue] = useState(initial); return [value, setValue]; }
function useInventoryApi() { const auth = useAuth(); return useMemo(() => inventoryApi(createApiClient(auth.getToken)), [auth.getToken]); }

export default function App() { return <QueryClientProvider client={queryClient}><AuthProvider><BrowserRouter><Routes><Route path="/login" element={<Login />} /><Route element={<ProtectedRoute />}><Route element={<Layout />}><Route index element={<Dashboard />} /><Route path="products" element={<Products />} /><Route path="categories" element={<Categories />} /><Route path="movements" element={<Movements />} /></Route></Route><Route path="*" element={<Navigate to="/" replace />} /></Routes></BrowserRouter></AuthProvider></QueryClientProvider>; }
