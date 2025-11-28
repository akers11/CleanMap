$(document).ready(function () {
    // Проверка загрузки Inputmask
    console.log("Inputmask доступен?", typeof Inputmask !== 'undefined');
    if (typeof Inputmask !== 'undefined') {
        console.log("Инициализация маски для #phoneNumber");
        Inputmask({ mask: "+7 (999) 999-99-99" }).mask("#phoneNumber");
    } else {
        console.error("Inputmask is not loaded.");
    }

    // Обработчик отправки формы регистрации
    $("#registerForm").on("submit", function (event) {
        const email = $("#email").val();
        const fullName = $("#fullName").val();
        const city = $("#city").val();
        const phoneNumber = $("#phoneNumber").val();

        console.log("Форма отправляется...");

        let isValid = true;

        // Проверка ФИО
        if (!/^[А-Яа-яЁё\s]+$/.test(fullName)) {
            alert("ФИО может содержать только буквы.");
            console.log("Ошибка в ФИО");
            isValid = false;
        }

        

        // Проверка номера телефона
        if (!phoneNumber || phoneNumber.includes("_")) {
            alert("Номер телефона должен быть полностью заполнен.");
            isValid = false;
        }

        // Проверка электронной почты
        if (!email.includes("@") || !email.includes(".")) {
            alert("Электронная почта должна содержать символы '@' и '.'.");
            console.log("Ошибка в почте");
            isValid = false;
        }

        // Если есть ошибки, отменяем отправку формы
        if (!isValid) {
            event.preventDefault();
            console.log("Форма не отправлена из-за ошибок.");
        } else {
            console.log("Форма валидна, отправляем...");
        }

        // Отладочные сообщения
        console.log("Проверка ФИО:", /^[А-Яа-яЁё\s]+$/.test(fullName));
        console.log("Проверка города:", /^[А-Яа-яЁё\s]+$/.test(city));
        console.log("Проверка номера телефона:", !phoneNumber || phoneNumber.includes("_"));
        console.log("Проверка почты:", !email.includes("@") || !email.includes("."));
    });
});