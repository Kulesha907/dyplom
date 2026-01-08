using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// TimeController.cs
// Ten skrypt sprawdza aktualną godzinę (rzeczywistą lub symulowaną)
// i wywołuje przypisany UnityEvent, gdy czas wchodzi w dany zakres godzin.

/// <summary>
/// Serializable container przechowujący zakres godzin oraz UnityEvent wywoływany przy wejściu w ten zakres.
/// </summary>
[Serializable]
public class TimeRangeEvent
{
    // Początek zakresu (inclusive) w godzinach 0..24 (np. 6.5 = 6:30)
    [Tooltip("Początek zakresu w godzinach (0..24), inclusive")]
    public float startHour;

    // Koniec zakresu (exclusive) w godzinach 0..24; jeśli end <= start, zakres "owija" się przez północ
    [Tooltip("Koniec zakresu w godzinach (0..24), exclusive; jeśli <= start, zakres owija się przez północ")]
    public float endHour;

    // Event wywoływany raz, gdy czas wejdzie w ten zakres
    [Tooltip("Event wywoływany przy wejściu w ten zakres godzin")]
    public UnityEvent onEnter;
}

/// <summary>
/// MonoBehaviour: sprawdza godzinę co określony interwał i wywołuje onEnter dla zakresu,
/// gdy właśnie weszliśmy do niego (nie wywołuje ciągle, tylko przy przejściu).
///
/// Jak używać:
/// - Dodaj komponent do GameObject w scenie.
/// - W inspectorze skonfiguruj listę `ranges` (7 zakresów od 6:00 do 21:00 są utworzone domyślnie,
///   jeśli nic nie ustawisz) i przypisz do każdego UnityEvent (np. wywołanie metody z innego komponentu).
/// - Przełącz `useRealTime` na true, aby używać systemowego czasu.
/// - Dla testów ustaw `useRealTime` = false i ustaw `simulatedHour`.
/// </summary>
public class TimeController : MonoBehaviour
{
    // Jeśli true, używamy rzeczywistego czasu systemowego (DateTime.Now)
    [Tooltip("Jeśli true, używa czasu systemowego. Jeśli false - używa simulatedHour (do testów)")]
    public bool useRealTime = false;

    // Godzina symulowana w przedziale [0,24). Przydatne do testów w edytorze.
    [Tooltip("Godzina w formacie 0..24 używana, gdy useRealTime == false (np. 6.5 = 6:30)")]
    public float simulatedHour = 6f;

    // Co ile sekund sprawdzać aktualną godzinę (np. 1s)
    [Tooltip("Interwał w sekundach między kolejnymi sprawdzeniami")]
    public float checkInterval = 1f;

    // Lista zakresów godzin z eventami (można ustawić ręcznie w inspectorze)
    [Tooltip("Lista zakresów godzin oraz eventów wywoływanych przy wejściu w zakres")]
    public List<TimeRangeEvent> ranges = new List<TimeRangeEvent>();

    // Indeks ostatnio aktywnego zakresu (-1 oznacza brak)
    int lastActiveIndex = -1;

    // Pomocniczy licznik czasu do obsługi checkInterval
    float timer = 0f;

    // Awake: jeżeli nie ustawiono zakresów w inspectorze, tworzymy domyślną konfigurację 7 zakresów
    void Awake()
    {
        if (ranges == null || ranges.Count == 0)
            CreateDefaultRanges();
    }

    // Update: co checkInterval sprawdzamy godzinę i wywołujemy event tylko gdy zmieni się aktywny zakres
    void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        // Pobierz godzinę - rzeczywistą albo symulowaną
        float hour = useRealTime ? (float)DateTime.Now.TimeOfDay.TotalHours : simulatedHour;

        // Sprawdź, w którym zakresie aktualnie jesteśmy
        int idx = GetRangeIndex(hour);

        // Jeśli indeks się zmienił (weszliśmy do nowego zakresu lub wyszliśmy z wszystkich), wywołaj event
        if (idx != lastActiveIndex)
        {
            if (idx >= 0 && ranges[idx] != null && ranges[idx].onEnter != null)
            {
                // Wywołaj przypisany event
                ranges[idx].onEnter.Invoke();
            }

            // Zaktualizuj ostatnio aktywny indeks
            lastActiveIndex = idx;
        }
    }

    // Zwraca indeks pierwszego zakresu, który zawiera daną godzinę, lub -1 jeśli żaden
    int GetRangeIndex(float hour)
    {
        hour = NormalizeHour(hour);
        for (int i = 0; i < ranges.Count; i++)
        {
            var r = ranges[i];
            if (r == null) continue;
            if (IsInRange(hour, r.startHour, r.endHour))
                return i;
        }
        return -1;
    }

    // Sprawdza, czy godzina mieści się w przedziale start..end.
    // Obsługuje także sytuację, gdy end <= start (zakres owija się przez północ), np. 22:00-02:00
    bool IsInRange(float hour, float start, float end)
    {
        start = NormalizeHour(start);
        end = NormalizeHour(end);

        if (Mathf.Approximately(start, end))
        {
            // Jeśli start == end: traktujemy to jako pełny dzień (0..24) - żeby uniknąć "pustego" zakresu
            return true;
        }

        if (end > start)
            return hour >= start && hour < end;

        // Zakres owija się przez północ (np. 23:00 - 03:00)
        return hour >= start || hour < end;
    }

    // Normalizuje godzinę do przedziału [0,24)
    float NormalizeHour(float h)
    {
        h %= 24f;
        if (h < 0f) h += 24f;
        return h;
    }

    // Tworzy domyślną listę 7 równych zakresów od 6:00 do 21:00
    void CreateDefaultRanges()
    {
        ranges = new List<TimeRangeEvent>();
        float start = 6f;
        float total = 15f; // od 6:00 do 21:00 -> 15 godzin
        int count = 7;     // 7 eventów
        float len = total / count; // długość pojedynczego zakresu

        for (int i = 0; i < count; i++)
        {
            float s = start + i * len;
            float e = (i == count - 1) ? 21f : s + len;

            var tre = new TimeRangeEvent
            {
                startHour = s,
                endHour = e,
                onEnter = new UnityEvent()
            };

            ranges.Add(tre);
        }
    }

    // Metoda publiczna: natychmiast sprawdza obecną godzinę i wywołuje event (użyteczne w edytorze/na przycisk)
    public void ForceCheck()
    {
        float hour = useRealTime ? (float)DateTime.Now.TimeOfDay.TotalHours : simulatedHour;
        int idx = GetRangeIndex(hour);
        if (idx >= 0 && ranges[idx] != null && ranges[idx].onEnter != null)
            ranges[idx].onEnter.Invoke();
        lastActiveIndex = idx;
    }
}

