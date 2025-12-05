import { describe, it, expect } from 'vitest';
import { formatDate, truncate } from './index';

describe('utils', () => {
  describe('formatDate', () => {
    it('should format a date correctly', () => {
      const date = new Date('2024-01-15T10:30:00');
      const result = formatDate(date);
      expect(result).toContain('2024');
      expect(result).toContain('January');
    });
  });

  describe('truncate', () => {
    it('should not truncate strings shorter than maxLength', () => {
      expect(truncate('Hello', 10)).toBe('Hello');
    });

    it('should truncate strings longer than maxLength', () => {
      expect(truncate('Hello World', 8)).toBe('Hello...');
    });

    it('should handle exact length', () => {
      expect(truncate('Hello', 5)).toBe('Hello');
    });
  });
});
