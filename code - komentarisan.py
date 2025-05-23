import pandas as pd

# 📥 Učitavanje podataka iz Excel fajla
# Pandas biblioteka se koristi za rad s tabelarnim podacima (DataFrame).
df = pd.read_excel("podaci3.xlsx")

# 📊 Računamo prvi (25%) i treći (75%) kvartil kolone 'OutletSales'
# Ovo nam pomaže da podijelimo prodaju na nisku, srednju i visoku (klasifikacija u 3 klase)
q1 = df["OutletSales"].quantile(0.25)
q3 = df["OutletSales"].quantile(0.75)

# 🎯 Funkcija koja određuje kojoj kategoriji pripada vrijednost prodaje (low/medium/high)
def kategorizuj(sales):
    if sales < q1:
        return "Low"      # Ako je ispod prvog kvartila, to je niska prodaja
    elif sales > q3:
        return "High"     # Ako je iznad trećeg kvartila, to je visoka prodaja
    else:
        return "Medium"   # Sve između je srednja prodaja

# ✅ Dodajemo novu kolonu u DataFrame koja sadrži našu ciljnu varijablu
# Ova kolona će se koristiti kao 'y' tj. ono što pokušavamo predvidjeti
df['SalesCategory'] = df['OutletSales'].apply(kategorizuj)

# 🧹 Popunjavanje praznih vrijednosti u koloni 'Weight'
# Prvo se prazne vrijednosti popunjavaju medianom unutar grupe (ProductType + OutletType)
df["Weight"] = df.groupby(["ProductType", "OutletType"])["Weight"].transform(lambda x: x.fillna(x.median()))

# Ako još ima praznih vrijednosti (npr. ako neka grupa nije imala dovoljno podataka), popuni ih globalnom median vrijednošću
df["Weight"] = df["Weight"].fillna(df["Weight"].median())

# 📤 Odvajanje podataka u "ulazne" (x) i "ciljne" (y)
# x = sve kolone osim 'SalesCategory' i 'OutletSales' (jer njih nećemo koristiti za predikciju)
# y = vrijednosti koje želimo predvidjeti (low, medium, high)
x = df.drop(columns=["SalesCategory", "OutletSales"])
y = df["SalesCategory"]

# 🔍 Razdvajanje kolona po tipu:
# numeričke vrijednosti (npr. MRP, Weight) i kategorijalne vrijednosti (npr. OutletType)
numericke_kolone = x.select_dtypes(include=["int64", "float64"]).columns.tolist()
kategorske_kolone = x.select_dtypes(include=["object", "category"]).columns.tolist()

# 🔡 Pretvaranje kategorijalnih podataka u brojeve pomoću "One-hot encoding"
# To znači da će svaka vrijednost iz npr. OutletType postati posebna kolona s 0 ili 1
x_kategorije = pd.get_dummies(x[kategorske_kolone])

# 🧩 Spajanje svih numeričkih i kategorijalno kodiranih kolona u jedan finalni skup
x_final = pd.concat([x[numericke_kolone], x_kategorije], axis=1)

from sklearn.preprocessing import StandardScaler
scaler = StandardScaler()

# ⚖️ Skaliranje (standardizacija) podataka
# Vrlo važno za neuralne mreže – sve vrijednosti se prebacuju na približno isti raspon (sredina = 0, odstupanje = 1)
x_scaled = scaler.fit_transform(x_final)

from sklearn.model_selection import train_test_split

# 🧪 Podjela na trening i test skup
# Treniramo model na 80% podataka i testiramo na preostalih 20%
# Stratify=y osigurava da sve klase (Low, Medium, High) budu ravnomjerno zastupljene
x_train, x_test, y_train, y_test = train_test_split(x_scaled, y, test_size=0.2, stratify=y, random_state=42)

from sklearn.neural_network import MLPClassifier

# 🧠 MLPClassifier = Multi-Layer Perceptron (neuralna mreža)
# hidden_layer_sizes=(100,) znači da postoji jedan skriveni sloj sa 100 neurona
# max_iter=500 = koliko puta model pokušava naučiti (maksimalni broj epoha)
model = MLPClassifier(hidden_layer_sizes=(100,), max_iter=500, random_state=42)

# 🏋️ Treniranje modela na trenirajućem skupu
model.fit(x_train, y_train)

# 📈 Predviđanje na test skupu (model pokušava pogoditi klase na osnovu naučenog)
y_pred = model.predict(x_test)

from sklearn.metrics import classification_report

# 🧾 Generisanje izvještaja o preciznosti, odzivu i F1-skóru
# Preciznost: koliko je tačnih predviđanja bilo od svih koja su data za tu klasu
# Odziv (recall): koliko tačnih predviđanja je model dao u odnosu na stvarni broj te klase
# F1-score: balans između preciznosti i odziva
report = classification_report(y_test, y_pred, output_dict=True)

# 📊 Prikaz izvještaja kao pregledne tabele sa zaokruženim vrijednostima
print(pd.DataFrame(report).transpose().round(2))
