import React, { useState } from 'react';

import './../styles/InlineEdit.css';

const InlineEdit = ({ value, onChange, name, as = 'input', className = '', ...props }) => {
  const [isEditing, setIsEditing] = useState(false);

  const handleFocus = (e) => {
    // Автоматично виділяємо текст в полі вводу
    e.target.select();
  };

  // 1. Якщо ми в режимі редагування, показуємо поле
  if (isEditing) {
    // Рендеримо або <input> або <textarea>
    const Component = as; // 'input' або 'textarea'
    
    return (
      <Component
        name={name}
        value={value}
        onChange={onChange}
        onBlur={() => setIsEditing(false)} // Коли клікнули "повз", повертаємо текст
        className={className}
        autoFocus // Автоматично фокусуємось на полі
        onFocus={handleFocus}
        {...props} // Передаємо решту (напр. placeholder)
      />
    );
  }

  // 2. Якщо ми НЕ в режимі редагування, показуємо текст
  // 'as' prop тут не використовується, рендеримо <p> або <h1>
  // (але ми керуємо цим через className)
  return (
    <div 
      className={className + " inline-editable-text"} // Додаємо клас для стилів
      onClick={() => setIsEditing(true)} // Клік для початку редагування
    >
      {value || props.placeholder || "Натисніть, щоб змінити"}
    </div>
  );
};

export default InlineEdit;