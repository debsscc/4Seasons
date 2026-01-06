import re
from collections import Counter

file_path = 'Event4.1.unity'  

pattern = re.compile(r'&(\d+)')

def find_duplicate_ids(file_path):
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    ids = pattern.findall(content)

    counts = Counter(ids)

    duplicates = {id_: count for id_, count in counts.items() if count > 1}

    return duplicates

if __name__ == '__main__':
    duplicates = find_duplicate_ids(file_path)
    if duplicates:
        print("IDs duplicados encontrados:")
        for id_, count in duplicates.items():
            print(f"ID: {id_} - Aparece {count} vezes")
    else:
        print("Nenhum ID duplicado encontrado.")