import { NavigationIcon } from './NavigationIcon'
import type { StaffNavigationItem, StaffSection } from './navigationItems'

type Props = {
  items: StaffNavigationItem[]
  selected: StaffSection
  onSelect: (section: StaffSection) => void
}

export function StaffNavigation({ items, selected, onSelect }: Props) {
  const groups = [...new Set(items.map(item => item.group))]
  return <nav className="staff-navigation" aria-label="Navegación principal">
    {groups.map(group => <div className="staff-nav-group" key={group}>
      <p className="staff-nav-label">{group}</p>
      <ul>
        {items.filter(item => item.group === group).map(item => <li key={item.id}>
          <button type="button" className="staff-nav-link" aria-current={selected === item.id ? 'page' : undefined}
            onClick={() => onSelect(item.id)}>
            <NavigationIcon section={item.id} /><span>{item.label}</span>
          </button>
        </li>)}
      </ul>
    </div>)}
  </nav>
}