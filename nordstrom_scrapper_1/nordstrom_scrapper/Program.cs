using System;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

public class Nordstrom : IShopScrapper
{
	ArrayList catResult = new ArrayList();
	static void Main(string[] args)
	{
		var url =
			"http://shop.nordstrom.com/c/mens-coats-jackets?origin=topnav&cm_sp=Top%20Navigation-_-Men-_-Clothing_Coats%20%26%20Jackets";

		Nordstrom n = new Nordstrom();
		var linksToProducts = n.GetProductsLinksFromCat(url);
		Console.WriteLine();
		//Console.WriteLine($"Collected {linksToProducts.Count} links");
		Console.ReadLine();
	}

	public string curl(string url1,string param1)
	{
		var url = "http://shop.nordstrom.com/soajax/storeavailability?postalCode=90067&radius=100";
		var request = (HttpWebRequest)WebRequest.Create(url);
		request.Method = WebRequestMethods.Http.Post;
		request.Headers["origin"] = "http://shop.nordstrom.com";
		request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
		request.Headers["Accept-Language"] = "en-US,en;q=0.8";
		request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Ubuntu Chromium/49.0.2623.108 Chrome/49.0.2623.108 Safari/537.36";
		request.ContentType = "application/json";
		request.Accept = "application/json";
		request.Referer = url1;
		using (var writer = new StreamWriter(request.GetRequestStream()))
		{
			writer.Write(param1);
		}

		var response = request.GetResponse();
		using (var reader = new StreamReader(response.GetResponseStream()))
		{
			var json = reader.ReadToEnd();
			return json;
		}
	}

	public string lib (string url)
	{
		WebClient client = new WebClient ();

		// Add a user agent header in case the 
		// requested URI contains a query.

		client.Headers.Add ("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

		Stream data = client.OpenRead (url);
		StreamReader reader = new StreamReader (data);
		string s = reader.ReadToEnd ();
		return s;




	}
	public string ShopName => "Nordstrom";

	public string GetProductId(string url)
	{
		string data = lib (url);

		Match conf_id = Regex.Match (data, @"productID\""\:(\d+)", RegexOptions.IgnoreCase);
		string sku = conf_id.Groups [1].Value;
		//Console.Write (IdInShop);
		return sku;
	}

	public void SetUsMode()
	{
		throw new NotImplementedException();
	}

	private void GetLinksFromPage(string catUrl, List<String> catResult)
	{
		string data = lib (catUrl);
		Console.Write (catUrl+ "This is what i found");
		Match product_urls_a = Regex.Match (data, @"(ProductResult"":{.*?<\/script)", RegexOptions.IgnoreCase);
		if (product_urls_a.Success) {
			string product_urls_a_1 = product_urls_a.Groups [1].Value;
			MatchCollection product_urls = Regex.Matches(product_urls_a_1, @"(ProductPageUrl\""\:\""[^\""]*\"")");
			foreach (Match product_url in product_urls) {


				string product_url_1 = product_url.ToString ();
				Match product_url_1_1 = Regex.Match (product_url_1, @"ProductPageUrl\""\:\""([^\""]*)\""", RegexOptions.IgnoreCase);
				if (product_url_1_1.Success) {
					//Console.WriteLine ("images: {0}", v_images.Groups [1].Value);
					string url_1 = product_url_1_1.Groups [1].Value;
					string url = "http://shop.nordstrom.com/" + url_1;
					catResult.Add(url);
					Console.Write (url+"\n");

				} 


			}
		}

		Match pagination_regex = Regex.Match (data, @"caret\""\shref\=\""([^\""]*)\"".*?\>Next", RegexOptions.IgnoreCase);
		Match pagination_regex1 = Regex.Match (data, @"Previous.*?caret\""\shref\=\""([^\""]*)\"".*?\>Next", RegexOptions.IgnoreCase);
		if (pagination_regex1.Success) {
			//string url = @"http://active.com";
			Regex regex = new Regex(@"([^\?]*)\?.*");
			string url1 = regex.Replace(catUrl, "$1");

			string pagination_value = pagination_regex1.Groups [1].Value;
			string paginationUrl = url1 + pagination_value;
			Console.Write (paginationUrl);
			GetLinksFromPage (paginationUrl,catResult);
		}
		else if (pagination_regex.Success) {
			//string url = @"http://active.com";
			Regex regex = new Regex(@"([^\?]*)\?.*");
			string url1 = regex.Replace(catUrl, "$1");

			string pagination_value = pagination_regex.Groups [1].Value;
			string paginationUrl = url1 + pagination_value;
			Console.Write (paginationUrl);
			GetLinksFromPage (paginationUrl,catResult);
		}
	}

	public List<string> GetProductsLinksFromCat(string catUrl)
	{
		var result = new List<String>();
		GetLinksFromPage(catUrl, result);
		return result;

	}


	/* public List<string> GetProductsLinksFromCat(string catUrl)
	{
		
		var result = new List<String>();
		string data = lib (catUrl);
		Console.Write (catUrl+ "This is what i found");
		Match product_urls_a = Regex.Match (data, @"(Products\""\:\[\{\"".*?],""SortOptions)", RegexOptions.IgnoreCase);
		if (product_urls_a.Success) {
			string product_urls_a_1 = product_urls_a.Groups [1].Value;
			MatchCollection product_urls = Regex.Matches(product_urls_a_1, @"(ProductPageUrl\""\:\""[^\""]*\"")");
			foreach (Match product_url in product_urls) {


				string product_url_1 = product_url.ToString ();
				Match product_url_1_1 = Regex.Match (product_url_1, @"ProductPageUrl\""\:\""([^\""]*)\""", RegexOptions.IgnoreCase);
				if (product_url_1_1.Success) {
					//Console.WriteLine ("images: {0}", v_images.Groups [1].Value);
					string url = product_url_1_1.Groups [1].Value;
					catResult.Add(url);
					//Console.Write (url);

				} 


			}
		}

		Match pagination_regex = Regex.Match (data, @"caret\""\shref\=\""([^\""]*)\"".*?\>Next", RegexOptions.IgnoreCase);
		Match pagination_regex1 = Regex.Match (data, @"Previous.*?caret\""\shref\=\""([^\""]*)\"".*?\>Next", RegexOptions.IgnoreCase);
		if (pagination_regex1.Success) {
			//string url = @"http://active.com";
			Regex regex = new Regex(@"([^\?]*)\?.*");
			string url1 = regex.Replace(catUrl, "$1");

			string pagination_value = pagination_regex1.Groups [1].Value;
			string paginationUrl = url1 + pagination_value;
			//Console.Write (paginationUrl);
			GetProductsLinksFromCat (paginationUrl,catResult);
		}
		else if (pagination_regex.Success) {
			//string url = @"http://active.com";
			Regex regex = new Regex(@"([^\?]*)\?.*");
			string url1 = regex.Replace(catUrl, "$1");

			string pagination_value = pagination_regex.Groups [1].Value;
			string paginationUrl = url1 + pagination_value;
			//Console.Write (paginationUrl);
			GetProductsLinksFromCat (paginationUrl,catResult);
		}
		int count = 0;
		foreach(var resul in catResult){
			count = count + 1;
			Console.Write (count);
			Console.Write (resul);
			result.Add(resul.ToString());
		}
		return result;
	} */

	public GetProductFromUrlResult GetProductFromUrl(string prodUrl)
	{
		var stores = new[] {"190","61","57"};
		
		ProductImportModel resultProduct = new ProductImportModel();
		var variations_array = new Dictionary<string, List<string>>();
		var variations_hash = new Dictionary<string, List<string>>();
		var variations_images = new Dictionary<string, List<string>>();
		ArrayList conf_list = new ArrayList();
		int has = 3;
		int has_var = 0;
		string data = lib (prodUrl);

		Match variations_image_ex = Regex.Match (data, @"StyleMedia\""\:\[(.*?)\]", RegexOptions.IgnoreCase);
		if (variations_image_ex.Success) {
			string variations_image_1 = variations_image_ex.Groups [1].Value;
			MatchCollection mc_images = Regex.Matches(variations_image_1, @"(Zoom\""\:\s*\""[^\""]+\""\,.*?\""MediaGroupType\""\:\s*\""Main\""\,.*?\""ColorName\"":\s*\""[^\""]+\""\,)");
			foreach (Match m_images in mc_images) {
				ArrayList item_array_images = new ArrayList();

				string s_images = m_images.ToString ();
				Match v_images = Regex.Match (s_images, @"Zoom\""\:\s*\""([^\""]+)\""", RegexOptions.IgnoreCase);
				if (v_images.Success) {
					//Console.WriteLine ("images: {0}", v_images.Groups [1].Value);
					string v_extracted_image = v_images.Groups [1].Value;
					item_array_images.Add (v_extracted_image.ToString());

				} else {
					item_array_images.Add ("");

				}

				Match v_images_name = Regex.Match (s_images, @"Zoom\""\:\s*\""[^\""]+\"".*?ColorName\"":\s*\""([^\""]+)\""", RegexOptions.IgnoreCase);
				if (v_images_name.Success) {
					//Console.WriteLine ("images_name: {0}", v_images_name.Groups [1].Value);
					string v_extracted_image_name = v_images_name.Groups [1].Value;
					item_array_images.Add (v_extracted_image_name.ToString());

				} else {
					item_array_images.Add ("");

				}
				string color_name_image = item_array_images [1].ToString();
				string color_image_image = item_array_images [0].ToString();

				variations_images[color_name_image] = new List<string> { color_image_image };
				//variations_images.Add (item_array_images[1], item_array_images[0]);
			}
		}

		Match variations_image_ex_1 = Regex.Match (data, @"StyleMedia\""\:\[(.*?)\]", RegexOptions.IgnoreCase);
		if (variations_image_ex_1.Success) {
			string variations_image_1 = variations_image_ex_1.Groups [1].Value;
			MatchCollection mc_images = Regex.Matches(variations_image_1, @"(MediaType\""\:\""Image\""\,\""MediaGroupType\""\:\""Alternate\"".*?\}.*?\})");
			foreach (Match m_images in mc_images) {
				ArrayList item_array_images = new ArrayList();

				string s_images = m_images.ToString ();
				Match v_images = Regex.Match (s_images, @"Zoom\""\:\s*\""([^\""]+)\""", RegexOptions.IgnoreCase);
				if (v_images.Success) {
					//Console.WriteLine ("images: {0}", v_images.Groups [1].Value);
					string v_extracted_image = v_images.Groups [1].Value;
					item_array_images.Add (v_extracted_image.ToString());

				} else {
					item_array_images.Add ("");

				}

				Match v_images_name = Regex.Match (s_images, @"ColorName\""\:\""([^\""]*)\""", RegexOptions.IgnoreCase);
				if (v_images_name.Success) {
					//Console.WriteLine ("images_name: {0}", v_images_name.Groups [1].Value);
					string v_extracted_image_name = v_images_name.Groups [1].Value;
					item_array_images.Add (v_extracted_image_name.ToString());

				} else {
					item_array_images.Add ("");

				}
				string color_name_image = item_array_images [1].ToString();
				string color_image_image = item_array_images [0].ToString();
				List<String> list;
				if (variations_images.TryGetValue (color_name_image, out list)) {
					list.Add (color_image_image);
				}
				//variations_images[color_name_image] = new List<string> { color_image_image };
				//variations_images.Add (item_array_images[1], item_array_images[0]);
			}
		}

		Match variations = Regex.Match (data, @"Skus\"":\[(.*?)\]", RegexOptions.IgnoreCase);
		if (variations.Success) {
			string variations_1 = variations.Groups [1].Value;
			MatchCollection mc = Regex.Matches(variations_1, @"\{(.*?)\}");
			foreach (Match m in mc)
			{
				ArrayList item_array = new ArrayList();
				string s = m.ToString();

				Match v_id = Regex.Match (s, @"Id\""\:(\d+)", RegexOptions.IgnoreCase);
				if (v_id.Success) {
					//Console.WriteLine ("variation id: {0}", v_id.Groups [1].Value);
					string v_id_1 = v_id.Groups [1].Value;
					Match color = Regex.Match (s, @"color\""\:\""([^\""]*)\""", RegexOptions.IgnoreCase);
					if (color.Success) {
						//Console.WriteLine ("color: {0}", color.Groups [1].Value);
						string color_1 = color.Groups [1].Value;
						item_array.Add (color_1.ToString());
					} else {
						item_array.Add ("");
					}

					Match size = Regex.Match (s, @"size\""\:\""([^\""]*)\""", RegexOptions.IgnoreCase);
					if (size.Success) {
						//Console.WriteLine ("Size: {0}", size.Groups [1].Value);
						string size_1 = size.Groups [1].Value;
						item_array.Add (size_1.ToString());
					} else {
						item_array.Add ("");
					}

					Match price_1 = Regex.Match (s, @"price\""\:\""([^\""]*)\""", RegexOptions.IgnoreCase);
					if (price_1.Success) {
						//Console.WriteLine ("Price_1: {0}", price.Groups [1].Value);
						string price_1_1 = price_1.Groups [1].Value;
						item_array.Add (price_1_1.ToString());
					} else {
						item_array.Add ("");
					}
					string item1 = item_array[0].ToString();
					string item2 = item_array[1].ToString();
					string item3 = item_array[2].ToString();
					conf_list.Add (v_id_1);
					variations_array[v_id_1] = new List<string> { item1,item2,item3 };
					//List<String> list; 
					//if (variations_hash.TryGetValue (v_id_1, out list)) {
					//	list.Add (item1);
					//	list.Add (item2);
					//	list.Add (item3);
				}
				//else{
				//	variations_hash.Add(v_id_1, new List<String>() {item1});

				//}
				//variations_hash[v_id_1] = new List<string> { item3 };

				//}
				//string[] items_array_1  = item_array.ToArray();

			}

		}
		else {
			Console.Write ("Variations not found");
		}
		string conf = "[" + String.Join(",", conf_list.Cast<string>().ToList()) + "]";
		//string conf = "[32882914,32877390,32877377,32882917,32882922,32882926,32877379,32882887,32882891,32877382,32882897,32882902,32882907,32882909]";
		Match conf_id = Regex.Match (data, @"productID\""\:(\d+)", RegexOptions.IgnoreCase);


		string sku = conf_id.Groups [1].Value;

		string param1 = "{\"SameDayDeliveryStoreNumber\":0,\"styleSkus\":[{\"StyleId\":" + sku + ",\"SkuIds\":" + conf + "}],\"RefreshSameDayDeliveryStore\":true}";
		string data_1 = curl (prodUrl,param1);
		//Console.Write (data_1);

		Match variations_get_sku = Regex.Match (data_1, @"Skus\""\:\[(.*?\]\})\]\}\]", RegexOptions.IgnoreCase);
		if (variations_get_sku.Success) {
			string variations_get_1 = variations_get_sku.Groups [1].Value;
			MatchCollection loop_1 = Regex.Matches(variations_get_1, @"\{(.*?)\}");

			foreach (Match l_1 in loop_1) {
				
				
				string l_1_1 = l_1.ToString ();
				Console.Write (l_1_1+"\n");
				//Match variations_get_sku_id = Regex.Match (l_1_1, @"Stores\""\:\W(.*?)\W", RegexOptions.IgnoreCase);
				//if (variations_get_sku_id.Success) {
				//	string variations_get_1 = variations_get_sku.Groups [1].Value;
				//	MatchCollection loop_1 = Regex.Matches(variations_get_1, @"\{(.*?)\}");
				//	foreach (Match l_1 in loop_1) {
				//		Console.Read ();
				//		string l_1_1 = l_1.ToString ();
				//		Console.Write (l_1_1+"\n");
				ArrayList store_array_check_1 = new ArrayList();
				foreach (var store in stores) {
					string color_inside = "no";
					string size_inside = "size no";
					Match variations_get_s = Regex.Match (l_1_1, @"Stores\""\:\[(.*?)\]", RegexOptions.IgnoreCase);
				if (variations_get_s.Success) {
					//Console.Read ();
					string id_out_1 = variations_get_s.Groups [1].Value;
						MatchCollection loop_1_2 = Regex.Matches(id_out_1, @"(\d+)");
						foreach (Match l_1_2 in loop_1_2) {

							string l_1_2_2 = l_1_2.ToString ();
							Console.Write (l_1_2_2+"\n");

							string id_out = l_1_2_2;
						Console.Write (store+"- store\n");
						Console.Write (id_out+"- id\n");
						if (id_out.Contains (store)) {
							//Console.Write ("Success");
							Match variations_get_check_s = Regex.Match (l_1_1, @"Id\""\:(\d+)\,\""Stores", RegexOptions.IgnoreCase);
							if (variations_get_check_s.Success) {
								string variations_check_data = variations_get_check_s.Groups [1].Value;
								if (variations_array.ContainsKey (variations_check_data)) {

									//ArrayList store_array_check_1 = new ArrayList ();
									has = 1;
										has_var = 1;
										color_inside = variations_array [variations_check_data] [0];
										size_inside = variations_array [variations_check_data] [1];
									//Console.Write (variations_array [variations_check_data] [0] + "\n");
									//Console.Write (variations_array [variations_check_data] [1] + "\n");
									//Console.Write ("inside" + id_out + "\n");
									if (id_out == "190") {
										//Console.Write ("Santa Monica Place, 220 Broadway, Santa Monica, CA 90401\n");
										string store_c = ("Santa Monica Place, 220 Broadway, Santa Monica, CA 90401");
											store_array_check_1.Add(store_c.ToString ());

										
									} else if (id_out == "61") {
										//Console.Write ("The Grove, 189 The Grove Dr, Los Angeles, CA 90036\n");

										string store_c = ("The Grove, 189 The Grove Dr, Los Angeles, CA 90036");
										store_array_check_1.Add (
											store_c.ToString ()

										);
									}
										else if (id_out == "57") {
											//Console.Write ("The Grove, 189 The Grove Dr, Los Angeles, CA 90036\n");

											string store_c = ("Westside Pavilion, 10830 W Pico Blvd, Los Angeles, CA 90064");
											store_array_check_1.Add (
												store_c.ToString ()

											);
										}
									
								}
							}
						}
						}

					}
					if (has_var == 1) {
						var colorVariant = new ProductImportColorVariantModel();
						var listOfStores = new List<StoreInfo>();
						colorVariant.Value = color_inside;
						foreach (var store_c in store_array_check_1) {
							Console.Write(store_c);
							listOfStores.Add (new StoreInfo {
								Name = store_c.ToString()

							});
						}
						AddColorAndSize (resultProduct, color_inside, size_inside, listOfStores);
						if (variations_images.ContainsKey (color_inside)) {
							AddImages(resultProduct, color_inside, variations_images [color_inside]);
							Console.Write (color_inside + "\n");
							Console.Write (size_inside + "\n");

							Console.Write (variations_images [color_inside]);
						}

				} 
					has_var = 0;


				}
			}



		}

		if (has == 1) {

			resultProduct.UrlInShop = prodUrl;
			resultProduct.IdInShop = GetProductId(prodUrl);

			Match name = Regex.Match (data, @"<h1[^>]*>([^<]*)<\/h1", RegexOptions.IgnoreCase);
			if (name.Success) {
				resultProduct.Name = name.Groups [1].Value;
				Console.WriteLine ("Name: {0}",name.Groups [1].Value);
			} else {
				Console.Write ("name not found");
			}

			Match brand = Regex.Match (data, @"brandName\""\:\""([^\""]*)\""", RegexOptions.IgnoreCase);
			if (brand.Success) {
				resultProduct.DesignerName = brand.Groups [1].Value;
				Console.WriteLine ("Brand/Designer: {0}",brand.Groups [1].Value);
			} else {
				Console.Write ("Brand/Designer not found");
			}

			Match description = Regex.Match (data, @"Description\""\:\""[<p>]*(.*?)[<\/p>]*\""\,", RegexOptions.IgnoreCase);
			if (description.Success) {
				resultProduct.DescriptionLines.Add (description.Groups [1].Value);
				Console.WriteLine ("Description: {0}",description.Groups [1].Value);
			} else {
				Console.Write ("description not found");

			}

			Match des_add = Regex.Match (data, @"<div itemprop=""description"".*?<\/p><\/div><ul[^>]*>(.*?)<\/ul", RegexOptions.IgnoreCase);
			if (des_add.Success) {
				string des_add_1 = des_add.Groups [1].Value;
				MatchCollection des_loop = Regex.Matches(des_add_1, @"(<li[^>]*>[^<]*<\/li)");
				foreach (Match description_add in des_loop) {
					string description_add_1 = description_add.ToString ();
					Match v_desc = Regex.Match (description_add_1, @"<li[^>]*>([^<]*)<\/li", RegexOptions.IgnoreCase);
					if (v_desc.Success) {
						//Console.WriteLine ("images: {0}", v_images.Groups [1].Value);
						string v_extracted_desc = v_desc.Groups [1].Value;
						resultProduct.DescriptionLines.Add (v_extracted_desc.ToString ());
						Console.Write (v_extracted_desc.ToString ());

					}
				}
			}

			Match price = Regex.Match (data, @"salePrice\""\:\""\$[\d\.]*\s\W\s\$([^\""]*)\""", RegexOptions.IgnoreCase);
			Match price_1 = Regex.Match (data, @"salePrice\""\:\""\$([^\""]*)\""", RegexOptions.IgnoreCase);
			Match price_2 = Regex.Match (data, @"basePrice\""\:\""\$([^\""]*)\""", RegexOptions.IgnoreCase);
			Match sale_price = Regex.Match (data, @"basePrice\""\:\""\$([^\""]*)\""", RegexOptions.IgnoreCase);
			if (price.Success) {
				resultProduct.Price = Decimal.Parse (price.Groups [1].Value);
				Console.WriteLine ("Price: {0}", price.Groups [1].Value);

				if (sale_price.Success) {
					resultProduct.PreSalePrice = Decimal.Parse(sale_price.Groups [1].Value);
					Console.WriteLine ("Sale Price: {0}",sale_price.Groups [1].Value);
				} 
			} else if (price_1.Success) {
				resultProduct.Price = Decimal.Parse (price_1.Groups [1].Value);
				Console.WriteLine ("Price: {0}", price_1.Groups [1].Value);
				if (sale_price.Success) {
					resultProduct.PreSalePrice = Decimal.Parse(sale_price.Groups [1].Value);
					Console.WriteLine ("Sale Price: {0}",sale_price.Groups [1].Value);
				} 
			} else if (price_2.Success) {
				resultProduct.Price = Decimal.Parse (price_2.Groups [1].Value);
				Console.WriteLine ("Price: {0}", price_2.Groups [1].Value);
			} else {
				Console.Write ("Price not foind");
			}



		} else {
			Console.Write ("Product Not Available in Santa Monica Place, 220 Broadway, Santa Monica, CA 90401 / 10830 W Pico Blvd, Los Angeles, CA 90064 / The Grove, 189 The Grove Dr, Los Angeles, CA 90036");
		}




		/* Match variations_get_sku = Regex.Match (data_1, @"Skus\""\:\[(.*?\]\})\]\}\]", RegexOptions.IgnoreCase);
		if (variations_get_sku.Success) {
			string variations_get_1 = variations_get_sku.Groups [1].Value;
			MatchCollection loop_1 = Regex.Matches(variations_get_1, @"\{(.*?)\}");
			foreach (Match l_1 in loop_1) {
				string l_1_1 = l_1.ToString ();
				Match variations_get_s = Regex.Match (l_1_1, @"190|61", RegexOptions.IgnoreCase);
				if (variations_get_s.Success) {
					//Console.Write ("Success");
					Match variations_get_check_s = Regex.Match (l_1_1, @"Id\""\:(\d+)\,\""Stores", RegexOptions.IgnoreCase);
					if (variations_get_check_s.Success) {
						string variations_check_data = variations_get_check_s.Groups [1].Value;
						if (variations_array.ContainsKey(variations_check_data))
						{
							if (variations_hash.ContainsKey (variations_array[variations_check_data][0])) {

								List<String> list;
								if (variations_hash.TryGetValue (variations_array[variations_check_data][0], out list)) {
									list.Add (variations_array[variations_check_data][1]);
								}
								//variations_hash.Add (variations_array[variations_check_data][0], new List<String> () { variations_array[variations_check_data][1] });
							} else {
								
								variations_hash[variations_array[variations_check_data][0]] = new List<string> { variations_array[variations_check_data][1] };
							}

						}

					}
				} 


			}



		}


		 foreach (var job in variations_hash) {
			ProductImportColorVariantModel productImportColor = new ProductImportColorVariantModel();
			resultProduct.ColorVariants.Add(productImportColor);
			productImportColor.Value = job.Key;
			Console.Write (job.Key+"\n");
			if (variations_images.ContainsKey (job.Key)) {
				foreach (var images in variations_images[job.Key]) {
					productImportColor.AddImageUrl(images);
					Console.Write (images+"\n");


				}
			}
			foreach (string jobs in job.Value) {
				productImportColor.AddSize(jobs, true);
				Console.Write (jobs+"\n");

			}
		} */

		return new GetProductFromUrlResult(resultProduct);
	}

	private void AddImage(ProductImportModel product, String color, String imageUrl)
	{
		var colorVariant = product.ColorVariants.FirstOrDefault(x => x.Value == color); // I get color variant if it exists. If not. I need to create it and add.
		if (colorVariant == null)
		{
			colorVariant = new ProductImportColorVariantModel();
			colorVariant.Value = color;

			// And append to results
			product.ColorVariants.Add(colorVariant);

		}

		if (colorVariant.Images == null) colorVariant.Images = new List<string>();
		colorVariant.Images.Add(imageUrl);
	}

	private void AddImages(ProductImportModel product, String color, List<String> imageUrls)
	{
		foreach (var imageUrl in imageUrls)
		{
			AddImage(product, color, imageUrl);
		}
	}

	private void AddColorAndSize(ProductImportModel product, String color, String size,
		List<StoreInfo> sizeAvailableInStores)
	{
		var colorVariant = product.ColorVariants.FirstOrDefault(x => x.Value == color); // I get color variant if it exists. If not. I need to create it and add.
		if (colorVariant == null)
		{
			colorVariant = new ProductImportColorVariantModel();
			colorVariant.Value = color;

			// And append to results
			product.ColorVariants.Add(colorVariant);
		}

		colorVariant.AddSize(size, true, sizeAvailableInStores);
	}

	public List<Tuple<string, string>> GetAllColorFilters(string url)
	{
		var result = new List<Tuple<string, string>>();
		string data = lib (url);
		Match filter_types = Regex.Match (data, @"\<div id\=\""popup\-color\"" (.*?)<\/div", RegexOptions.IgnoreCase);
		if (filter_types.Success) {
			string filter_types_1 = filter_types.Groups [1].Value;
			MatchCollection filter_types_loop_1 = Regex.Matches(filter_types_1, @"(<a class=""filter-option"".*?<\/a)");
			foreach (Match filter_types_loop_l_1 in filter_types_loop_1) {
				string filter_types_loop_l_1_1 = filter_types_loop_l_1.ToString ();
				Match color_filter_loop = Regex.Match (filter_types_loop_l_1_1, @"<a class=""filter-option""\shref\=\""([^\""]*)\""[^>]*>([^<]*)<\/a", RegexOptions.IgnoreCase);
				if (color_filter_loop.Success) {
					//Console.Write ("Success");
					//string url = @"http://active.com";
					string color_filter_name = color_filter_loop.Groups [2].Value;
					string url_name = color_filter_loop.Groups [1].Value;
					string url_filter = "http://shop.nordstrom.com"+url_name;
					result.Add(Tuple.Create(color_filter_name, url_filter));

				} 
			}

		}
		return result;
	}


	public List<string> GetProductUrlsForColorFilter(string filter, string url)
	{
		var result = GetProductsLinksFromCat(url);
		return result;
	}

	public List<string> GetProductUrlsForAdditionalFilter(string filterName, string filterValue, string url)
	{
		throw new NotImplementedException();
	}
	/*class Exec
	{
		static void Main(string[] args) 
		{
			Nordstrom r = new Nordstrom();
			r.GetAllColorFilters ("http://shop.nordstrom.com/c/mens-polo-shirts?origin=topnav&cm_sp=Top%20Navigation-_-Men-_-Clothing_Polos?page=2");
			//Console.Write(r.curl ());
			Console.ReadLine(); 
		}
	}*/
}

public interface IShopScrapper
{
	string ShopName { get; }

	String GetProductId(string url); // Extract id of item from URL.

	void SetUsMode(); // If you don't need that leave it unimplemented. If you want to perform some actions like selecting website localization that's the place for it 

	List<string> GetProductsLinksFromCat(string catUrl); // Get all product links for 

	// Get info from product page
	GetProductFromUrlResult GetProductFromUrl(string prodUrl);
	// Product name
	// Product price
	// Designer/brand
	// Pre sale price
	// Product description
	// Id in shop
	// Colors with images + sizes for each color with availability info


	List<Tuple<String, String>> GetAllColorFilters(string url); // Tupple < Filter, Link > e.g. (Blue, http://www1.bloomingdales.com/shop/womens-apparel/coats/Color_normal,Productsperpage/Blue,180?id=1001520)

	List<String> GetProductUrlsForColorFilter(string filter, string url); // Return list of products for given color filter. In params you'll get one object from GetAllColorFilters function result.
	List<String> GetProductUrlsForAdditionalFilter(string filterName, String filterValue, string url);// Return list of products for given filter. Don't worry about other params than url.
}

public class GetProductFromUrlResult
{
	public List<ProductImportModel> ProductsList { get; set; }

	public ProductImportModel Product
	{
		get { return ProductsList?.FirstOrDefault(); }
		set
		{
			if (ProductsList == null) ProductsList = new List<ProductImportModel>();
			ProductsList.Add(value);
		}
	}

	public GetProductFromUrlResult(ProductImportModel product)
	{
		Product = product;
	}
}

public class ProductImportModel
{
	public bool IsAvailable { get; set; } = true;

	public string Name { get; set; }
	public string RetailerName { get; set; }

	public string DesignerName { get; set; }
	public Decimal Price { get; set; } // Current price

	public Decimal PreSalePrice { get; set; }
	public List<string> DescriptionLines { get; set; }
	public List<ProductImportColorVariantModel> ColorVariants { get; set; }

	public string IdInShop { get; set; }

	public string UrlInShop { get; set; }
	public List<string> ColorFilters { get; set; }



	public List<string> TypeFilters { get; set; } // Name, value. E.g Type, Pants 

	public ProductImportModel()
	{
		DescriptionLines = new List<string>();
		ColorVariants = new List<ProductImportColorVariantModel>();
		ColorFilters = new List<string>();
		TypeFilters = new List<string>();
	}

	// check if all colors are available
	public bool ifAllAvailable()
	{
		foreach (var color in ColorVariants)
		{
			if (!color.ifAllAreAvailable())
			{
				return false;
			}
		}

		return true;
	}

	public ProductImportColorVariantModel AddColorVariant(string text)
	{
		ProductImportColorVariantModel colorVariant = new ProductImportColorVariantModel();
		colorVariant.Value = text;
		ColorVariants.Add(colorVariant);
		return colorVariant;
	}



}

public class ProductImportColorVariantModel
{
	public string Value { get; set; }
	public List<string> Images { get; set; }

	public bool IsAvailable
	{
		get
		{
			if (SizeVariants == null || !SizeVariants.Any()) return false;
			return SizeVariants.Any(x => x.IsAvailable);
		}
	}

	public List<ProductImportSizeVariantsModel> SizeVariants;

	public ProductImportColorVariantModel()
	{
		Images = new List<string>();
		SizeVariants = new List<ProductImportSizeVariantsModel>();
	}

	public void AddImageUrl(String url)
	{
		Images.Add(url);
	}

	public void AddSize(String sizeValue, bool available, List<StoreInfo> availableInStores)
	{
		ProductImportSizeVariantsModel toAdd = new ProductImportSizeVariantsModel
		{
			AdditionalSizes = new List<ProductImportSizeVariantsModel>(),
			Value = sizeValue,
			IsAvailable = available,
			AvailableInStores = availableInStores
		};

		if (SizeVariants == null) SizeVariants = new List<ProductImportSizeVariantsModel>();
		SizeVariants.Add(toAdd);
	}

	// check if all sizes are available
	public bool ifAllAreAvailable()
	{
		foreach (var size in SizeVariants)
		{
			if (!size.IsAvailable)
			{
				return false;
			}
		}

		return true;
	}


}

public class ProductImportSizeVariantsModel
{
	public string Value { get; set; }
	public List<ProductImportSizeVariantsModel> AdditionalSizes; // Like widths of shoe for given size.
	public bool IsAvailable { get; set; } // is item available in ANY store of our interest ?

	public List<StoreInfo> AvailableInStores { get; set; } // Put here list of stores in which given size is available
}

public class StoreInfo
{
	public String Name { get; set; } // e.g.  The Grove
	public String FullAddress { get; set; } // e.g 189 The Grove Dr, Los Angeles, CA 90036
}
