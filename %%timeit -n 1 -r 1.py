
%%timeit -n 1 -r 1
from multiprocessing import Pool, cpu_count

def mark_non_primes_block(start, end, step, is_prime):
    for i in range(start, end, step):
        for j in range(i * i, end, i * 2):  # Шаг i*2, чтобы исключить чётные числа
            is_prime[j // 2] = False

def find_prime_groups(limit):
    """Находит группы простых чисел с разницей <= 2 и выводит их."""
    if limit < 3:
        return

    # Инициализация массива для нечётных чисел
    size = (limit // 2) + 1
    is_prime = [True] * size
    is_prime[0] = False  # 1 не является простым

    sqrt_limit = int(limit**0.5) + 1

    # Параллельная обработка блоков
    with Pool(processes=min(cpu_count(), 8)) as pool:
        block_size = max(1000, sqrt_limit)
        tasks = []
        for i in range(3, sqrt_limit, 2):  # Только нечётные числа
            if is_prime[i // 2]:
                tasks.append(pool.apply_async(mark_non_primes_block, args=(i, limit, 2 * i, is_prime)))
        for task in tasks:
            task.wait()

    # Формирование групп простых чисел
    prime_groups = []
    current_group = []

    for i in range(1, size):  # Перебор только нечётных чисел
        if is_prime[i]:
            prime = 2 * i + 1
            if not current_group or prime - current_group[-1] <= 2:
                current_group.append(prime)
            else:
                if len(current_group) > 1:
                    prime_groups.append(current_group)
                current_group = [prime]

    # Добавляем последнюю группу, если она подходит
    if len(current_group) > 1:
        prime_groups.append(current_group)

    # Выводим группы простых чисел
    for group in prime_groups:
        print(group)

if __name__ == "__main__":
    # Пример использования:
    find_prime_groups(1000000)