# hawkbit
Суть проекта обеспечить автоматическое обновление конфигурации
подключенных к сети устройств: администратор формирует пакет fboot 
файлов и публикует из через сервис (см https://www.eclipse.org/hawkbit/gettingstarted/)
Далее клиент загрузчика, работающий через rest api (см https://www.eclipse.org/hawkbit/apis/)
проверяет с заданной периодичностью наличие пакета обновлений, преобразует в
набор команд forte и загружает в рантайм устройства(см https://www.eclipse.org/4diac/en_rte.php).
Также возможна отправка обновлений рантайма exe и so файлов в устройства.
При отправке exe файла останавливается процесс и создается новый, а затем загружается конфигурация
При отправке so файла останавливается сервис через шину DBUS, меняется среда выполнения и запускается новый сервис,
а затем загружается конфигурация