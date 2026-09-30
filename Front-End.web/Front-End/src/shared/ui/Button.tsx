import type { ButtonHTMLAttributes } from 'react'
import './controls.css'

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement>
export function Button({ children, className = '', type = 'button', ...props }: ButtonProps) {
  return <button type={type} className={`button ${className}`} {...props}>{children}</button>
}