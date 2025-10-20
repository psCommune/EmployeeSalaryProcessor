<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
				xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
    <xsl:output method="xml" indent="yes"/>

	<!-- Шаблон для Data1.xml -->
	<xsl:template match="/Pay[item]">
		<Employees>
			<!-- Группируем по имени и фамилии -->
			<xsl:for-each select="item[not(@name = preceding-sibling::item/@name) or not(@surname = preceding-sibling::item/@surname)]">
				<xsl:variable name="currentName" select="@name"/>
				<xsl:variable name="currentSurname" select="@surname"/>

				<Employee name="{$currentName}" surname="{$currentSurname}">
					<!-- Выбираем все элементы с таким же именем и фамилией -->
					<xsl:for-each select="/Pay/item[@name = $currentName and @surname = $currentSurname]">
						<salary amount="{@amount}" mount="{@mount}"/>
					</xsl:for-each>
				</Employee>
			</xsl:for-each>
		</Employees>
	</xsl:template>

	<!-- Шаблон для Data2.xml -->
	<xsl:template match="/Pay[january or february or march]">
		<Employees>
			<!-- Собираем все уникальные комбинации имени и фамилии -->
			<xsl:variable name="allItems" select="*/item"/>

			<xsl:for-each select="$allItems[not(@name = preceding::item/@name) or not(@surname = preceding::item/@surname)]">
				<xsl:variable name="currentName" select="@name"/>
				<xsl:variable name="currentSurname" select="@surname"/>

				<Employee name="{$currentName}" surname="{$currentSurname}">
					<!-- Собираем все salary для данного сотрудника -->
					<xsl:for-each select="$allItems[@name = $currentName and @surname = $currentSurname]">
						<salary amount="{@amount}" mount="{@mount}"/>
					</xsl:for-each>
				</Employee>
			</xsl:for-each>
		</Employees>
	</xsl:template>
</xsl:stylesheet>
