wrk.method = "POST"

-- define random string generator
local charset = {}
-- qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890
for i = 48,  57 do table.insert(charset, string.char(i)) end
for i = 65,  90 do table.insert(charset, string.char(i)) end
for i = 97, 122 do table.insert(charset, string.char(i)) end
function string.random(length)
  math.randomseed(os.time())
  if length > 0 then
    return string.random(length - 1) .. charset[math.random(1, #charset)]
  else
    return ""
  end
end

local productCount = 0

request = function()
  local productGroupId = productCount % 2000 -- there are 2k product groups
  local productText = string.random(5) .. " Ä Ö Å ä ö å" -- test Swedish chars
  local longText = "This is a very very long text intentionally added to exceed One Kilobyte boundary."
  
  body = string.format([[
  {
    "id": "product_%s",
    "productGroupId": "productGroup_%s",
    "productText": "%s",
    "productStatusId": 11,
    "temperatureMax": -18,
    "temperatureMin": -23,
    "expiryDate": "19.02.2018 03:56:29",
    "VERY_LONG_ATTRIBUTE_1": "%s",
    "VERY_LONG_ATTRIBUTE_2": "%s",
    "VERY_LONG_ATTRIBUTE_3": "%s",
    "VERY_LONG_ATTRIBUTE_4": "%s",
    "VERY_LONG_ATTRIBUTE_5": "%s",
    "VERY_LONG_ATTRIBUTE_6": "%s",
    "VERY_LONG_ATTRIBUTE_7": "%s",
    "VERY_LONG_ATTRIBUTE_8": "%s",
    "VERY_LONG_ATTRIBUTE_9": "%s",
    "VERY_LONG_ATTRIBUTE_10": "%s"
  }
  ]], productCount, productGroupId, productText, 
  longText, longText, longText, longText, longText, longText, longText, longText, longText, longText)
  
  productCount = (productCount + 1) % 100000 -- there are 100k products
  
  return wrk.format(wrk.method, wrk.path, wrk.headers, body)
end