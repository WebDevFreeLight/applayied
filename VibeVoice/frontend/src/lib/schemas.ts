import { z } from 'zod'

/** Mirrors the API rule: min 8 chars, at least one letter and one digit. */
export function passwordSchema(message: string) {
  return z
    .string()
    .min(8, message)
    .regex(/\p{L}/u, message)
    .regex(/\d/, message)
}
