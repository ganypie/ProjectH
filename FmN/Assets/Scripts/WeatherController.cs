using UnityEngine;

public class WeatherController : MonoBehaviour
{
    public GameObject rainSystems;
    public GameObject fogSystem;

    public enum WeatherType { Clear, Rain, Fog }
    public WeatherType currentWeather = WeatherType.Clear;

    void UpdateWeather()
    {
        // Выключаем всё
        rainSystems.SetActive(false);
        fogSystem.SetActive(false);

        // Включаем выбранное
        switch (currentWeather)
        {
            case WeatherType.Rain:
                rainSystems.SetActive(true);
                break;
            case WeatherType.Fog:
                fogSystem.SetActive(true);
                break;
            case WeatherType.Clear:
            default:
                // ничего не включаем
                break;
        }
    }

    public void SetWeather(WeatherType newWeather)
    {
        currentWeather = newWeather;
        UpdateWeather();
    }
}
