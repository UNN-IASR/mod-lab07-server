import math
sum = 0
n = 5
λ = 29
m = 3
p = λ/m
for i in range(n + 1):
    sum += p**i/math.factorial(i)

P0 = 1/sum

Pn = ((p**n)*P0)/math.factorial(n)
A = λ*(1-Pn)
reret = A / m

print("Интенсивность p", p)
print("Вероятность простоя системы P0 : ", P0)
print("Вероятность отказа системы Pn : ", Pn)
print("Относительная пропускная способность Q :", 1 - Pn)
print("Абсолютная пропускная способность A  : ", A)
print("Среднее число занятых каналов k : ", A/m)
