export function validateCustomerRegistration(email: string, password: string, confirmation: string): string | null {
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim()) || email.trim().length > 254) return 'Ingresa un correo electrónico válido.'
  if (password.length < 8 || !/[a-z]/.test(password) || !/[A-Z]/.test(password) || !/[0-9]/.test(password))
    return 'La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número.'
  if (new TextEncoder().encode(password).length > 72) return 'La contraseña no puede superar 72 bytes en UTF-8.'
  if (password !== confirmation) return 'Las contraseñas no coinciden.'
  return null
}
