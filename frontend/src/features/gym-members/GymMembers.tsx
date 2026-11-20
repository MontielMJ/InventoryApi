import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { createApiClient } from '../../api/client';
import { inventoryApi } from '../../api/inventoryApi';
import { useAuth } from '../../auth/useAuth';
import type { GymMember, GymMemberInput, GymMemberStatus } from '../../types';

const statuses: Array<GymMemberStatus | 'All'> = ['All', 'Active', 'Inactive', 'Cancelled'];

export default function GymMembers() {
  const auth = useAuth();
  const api = useMemo(() => inventoryApi(createApiClient(auth.getToken)), [auth.getToken]);
  const queryClient = useQueryClient();
  const [status, setStatus] = useState<GymMemberStatus | 'All'>('All');
  const [search, setSearch] = useState('');
  const [editing, setEditing] = useState<GymMember | 'new' | null>(null);
  const members = useQuery({ queryKey: ['gym-members', status, search], queryFn: () => api.gymMembers.list(status === 'All' ? undefined : status, search) });
  const save = useMutation({ mutationFn: async (input: GymMemberInput) => { if (editing && editing !== 'new') await api.gymMembers.update(editing.id, input); else await api.gymMembers.create(input); }, onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['gym-members'] }); setEditing(null); } });
  const items = members.data?.items ?? [];
  const activeCount = items.filter(member => member.status === 'Active').length;
  const inactiveCount = items.filter(member => member.status === 'Inactive').length;
  const cancelledCount = items.filter(member => member.status === 'Cancelled').length;
  return <section><div className="page-heading"><div><span className="eyebrow">MEMBERSHIP</span><h2>Gym members</h2><p className="muted">Manage active, inactive and cancelled memberships.</p></div><button className="primary-button" onClick={() => setEditing('new')}>Add member <span>+</span></button></div><div className="stat-grid"><MemberStat label="Active" value={activeCount} tone="green" /><MemberStat label="Inactive" value={inactiveCount} tone="orange" /><MemberStat label="Cancelled" value={cancelledCount} tone="red" /></div><div className="toolbar"><input className="search-input" placeholder="Search name, number or email..." value={search} onChange={event => setSearch(event.target.value)} /><div className="status-tabs">{statuses.map(value => <button key={value} className={status === value ? 'status-tab selected' : 'status-tab'} onClick={() => setStatus(value)}>{value}</button>)}</div></div>{editing && <MemberForm initial={editing === 'new' ? undefined : editing} saving={save.isPending} onCancel={() => setEditing(null)} onSubmit={value => save.mutate(value)} />}{members.isLoading ? <div className="panel empty-state">Loading members...</div> : <div className="panel table-panel"><table><thead><tr><th>Member</th><th>Contact</th><th>Status</th><th>Start date</th><th>End date</th><th /></tr></thead><tbody>{items.map(member => <tr key={member.id}><td><div className="table-product"><div className="product-avatar">{member.fullName.slice(0, 1)}</div><div><strong>{member.fullName}</strong><small>{member.memberNumber}</small></div></div></td><td><div>{member.email}</div><small>{member.phone}</small></td><td><span className={`badge ${member.status === 'Active' ? 'success' : member.status === 'Inactive' ? 'inactive-badge' : 'out-badge'}`}>{member.status}</span></td><td>{formatDate(member.membershipStartDate)}</td><td>{member.membershipEndDate ? formatDate(member.membershipEndDate) : '—'}</td><td><button className="icon-button" onClick={() => setEditing(member)}>Edit</button></td></tr>)}</tbody></table>{items.length === 0 && <div className="empty-state">No members found.</div>}</div>}</section>;
}

function MemberForm({ initial, saving, onCancel, onSubmit }: { initial?: GymMember; saving: boolean; onCancel: () => void; onSubmit: (value: GymMemberInput) => void }) {
  const [form, setForm] = useState<GymMemberInput>({ memberNumber: initial?.memberNumber ?? '', fullName: initial?.fullName ?? '', email: initial?.email ?? '', phone: initial?.phone ?? '', status: initial?.status ?? 'Active', membershipStartDate: initial?.membershipStartDate.slice(0, 10) ?? new Date().toISOString().slice(0, 10), membershipEndDate: initial?.membershipEndDate?.slice(0, 10) ?? '', cancellationReason: initial?.cancellationReason ?? '' });
  return <form className="panel form-card" onSubmit={event => { event.preventDefault(); onSubmit(form); }}><div className="form-title"><h3>{initial ? 'Edit member' : 'New member'}</h3><button type="button" className="close-button" onClick={onCancel}>×</button></div><div className="form-grid"><Field label="Member number"><input required value={form.memberNumber} onChange={event => setForm({ ...form, memberNumber: event.target.value })} /></Field><Field label="Full name"><input required value={form.fullName} onChange={event => setForm({ ...form, fullName: event.target.value })} /></Field><Field label="Email"><input required type="email" value={form.email} onChange={event => setForm({ ...form, email: event.target.value })} /></Field><Field label="Phone"><input required value={form.phone} onChange={event => setForm({ ...form, phone: event.target.value })} /></Field><Field label="Status"><select value={form.status} onChange={event => setForm({ ...form, status: event.target.value as GymMemberStatus })}><option value="Active">Active</option><option value="Inactive">Inactive</option><option value="Cancelled">Cancelled</option></select></Field><Field label="Membership start"><input required type="date" value={form.membershipStartDate} onChange={event => setForm({ ...form, membershipStartDate: event.target.value })} /></Field><Field label="Membership end"><input type="date" value={form.membershipEndDate} onChange={event => setForm({ ...form, membershipEndDate: event.target.value })} /></Field><Field label="Cancellation reason"><input value={form.cancellationReason} onChange={event => setForm({ ...form, cancellationReason: event.target.value })} /></Field></div><div className="form-actions"><button type="button" className="secondary-button" onClick={onCancel}>Cancel</button><button className="primary-button" disabled={saving}>{saving ? 'Saving...' : 'Save'}</button></div></form>;
}

function Field({ label, children }: { label: string; children: ReactNode }) { return <label className="field"><span>{label}</span>{children}</label>; }
function MemberStat({ label, value, tone }: { label: string; value: number; tone: string }) { return <div className="stat-card member-stat"><div className={`stat-icon ${tone}`}>●</div><div><span>{label} members</span><strong>{value}</strong></div></div>; }
function formatDate(value: string) { return new Date(value).toLocaleDateString(); }
