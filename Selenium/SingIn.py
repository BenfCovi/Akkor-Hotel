import time
import random
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.alert import Alert
from selenium.webdriver.common.action_chains import ActionChains
from selenium.webdriver.chrome.service import Service
from webdriver_manager.chrome import ChromeDriverManager
from selenium.common.exceptions import NoSuchElementException
from selenium.webdriver.chrome.options import Options
random_number = random.randint(10000000, 99999999)

Email = str(random_number)+"@gmail.com"
Password = str(random_number)

driver = webdriver.Chrome()

driver.get("http://127.0.0.1:5500/index.html")

time.sleep(2)

# Create new account
driver.find_element(By.XPATH, '//*[@id="userActions"]/button[1]').click()
time.sleep(1)
text_area = driver.find_element(By.XPATH, '//*[@id="singinEmail"]')
text_area.clear()
text_area.send_keys(Email)
text_area = driver.find_element(By.XPATH, '//*[@id="signinPassword"]')
text_area.clear()
text_area.send_keys(Password)
text_area = driver.find_element(By.XPATH, '//*[@id="signinConfirmPassword"]')
text_area.clear()
text_area.send_keys(Password)
driver.find_element(By.XPATH, '//*[@id="singinButton"]').click()
# Validate alerte
time.sleep(2)
alert = driver.switch_to.alert
assert "Login successful!" in alert.text
alert.accept()
time.sleep(2)
alert = driver.switch_to.alert
assert "Registration successful! You are now logged in." in alert.text
alert.accept()
time.sleep(1)

# Go to profil
driver.find_element(By.XPATH, '//*[@id="userActions"]/a[2]').click()
time.sleep(1)

# Remove account
button = driver.find_element(By.XPATH, '//*[@id="deleteAccountButton"]')
ActionChains(driver).move_to_element(button).click(button).perform()
time.sleep(1)
alert = driver.switch_to.alert
alert.accept()

# Finish
driver.quit()